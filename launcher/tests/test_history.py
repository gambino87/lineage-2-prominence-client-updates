"""Check client-history comparisons against real releases and cumulative patch behavior."""
import copy
import json
from pathlib import Path
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from release import ROOT
from sync_history import file_changes


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


if __name__ == '__main__':
    unittest.main()
