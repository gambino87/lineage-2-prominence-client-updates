"""Check client-history comparisons against real releases and cumulative patch behavior."""
import copy
import json
from pathlib import Path
import sys
import subprocess
import tempfile
import unittest
from unittest.mock import patch
from types import SimpleNamespace

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from release import ROOT
from sync_history import file_changes
import sync_history


def manifest(version):
    return json.loads((ROOT / 'outputs/releases' / version / 'manifest.json').read_text())


class HistoryTests(unittest.TestCase):
    def test_first_release_lists_cumulative_patches(self):
        changes = file_changes(None, manifest('0.2.0'))
        self.assertEqual(len(changes), 18)
        self.assertTrue(all(row['Change'] == 'Initial patch' for row in changes))

    def test_recent_releases_identify_the_quest_journal(self):
        for previous, current in [('0.2.0', '0.2.1'), ('0.2.1', '0.2.2')]:
            changes = file_changes(manifest(previous), manifest(current))
            self.assertEqual([row['Path'] for row in changes], ['system/questname-e.dat'])
            self.assertNotEqual(changes[0]['BeforeSha256'], changes[0]['AfterSha256'])
            self.assertIn('/' + current + '/', changes[0]['Download'])

    def test_repacked_archives_are_not_reported_as_client_edits(self):
        previous = manifest('0.2.0')
        current = copy.deepcopy(previous)
        for row in current['Files']:
            if row.get('Asset'):
                row['AssetSha256'] = '0' * 64
                row['AssetSize'] += 1
        self.assertEqual(file_changes(previous, current), [])

    def test_removed_manifest_entry_is_not_claimed_deleted_on_disk(self):
        previous = manifest('0.2.0')
        current = copy.deepcopy(previous)
        removed = current['Files'].pop()
        changes = file_changes(previous, current)
        self.assertEqual(changes[0]['Path'], removed['Path'])
        self.assertEqual(changes[0]['Change'], 'No longer managed')
        self.assertIsNone(changes[0]['AfterSha256'])

    def test_git_conversion_of_signed_bytes_is_detected(self):
        with tempfile.TemporaryDirectory(prefix='l2-history-git-') as folder:
            root = Path(folder)
            subprocess.run(['git','init','-q',folder], check=True)
            subprocess.run(['git','-C',folder,'config','core.autocrlf','true'], check=True)
            relative = 'client-history/fixture/manifest.json'
            target = root / relative
            target.parent.mkdir(parents=True)
            target.write_bytes(b'{\r\n  "Version": "fixture"\r\n}\r\n')
            subprocess.run(['git','-C',folder,'add','--',relative], check=True, capture_output=True)
            with patch.object(sync_history, 'SOURCE_ROOT', root):
                with self.assertRaisesRegex(ValueError, 'signed metadata bytes'):
                    sync_history.verify_staged_manifests([relative])
                (root / '.gitattributes').write_text('client-history/**/manifest.json -text\n')
                subprocess.run(['git','-C',folder,'add','--renormalize','--',relative], check=True)
                sync_history.verify_staged_manifests([relative])

    def test_just_published_release_is_recorded_before_public_list_refreshes(self):
        repository = 'fixture/updates'
        remote = 'https://github.com/' + repository
        published = {'tag_name':'fixture-1','draft':False,'html_url':remote+'/releases/tag/fixture-1','published_at':'2026-09-08T00:00:00Z'}
        release = {'Version':'fixture-1','Notes':'Fixture release','AssetBaseUrl':remote+'/releases/download/fixture-1/',
                   'Files':[{'Path':'system/fixture.dat','Sha256':'a'*64,'Size':1,'Preserve':False,'Asset':'fixture.zip'}]}
        def download(url):
            if '/releases?' in url:
                return b'[]'
            if url.endswith('manifest.json.sig'):
                return b'fixture signature'
            return json.dumps(release).encode()
        with tempfile.TemporaryDirectory(prefix='l2-history-published-') as folder:
            root = Path(folder)
            with patch.object(sync_history,'SOURCE_ROOT',root), patch.object(sync_history,'STATE',root/'state'), patch.object(sync_history,'git',return_value=SimpleNamespace(stdout=remote)), patch.object(sync_history,'download',side_effect=download), patch.object(sync_history.subprocess,'run'):
                records = sync_history.sync_history(repository,published_release=published)
            self.assertEqual([row['Version'] for row in records],['fixture-1'])
            self.assertTrue((root/'client-history/fixture-1/changes.json').exists())


if __name__ == '__main__':
    unittest.main()
