"""Regression checks for staging and public-release publication safeguards."""
import json
from pathlib import Path
import sys
import tempfile
from types import SimpleNamespace
import unittest
import urllib.error
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import publish
import release
import validate_release

REPOSITORY = 'gambino87/lineage-2-prominence-client-updates'


class WorkflowTests(unittest.TestCase):
    def test_live_client_cannot_be_release_input(self):
        args = SimpleNamespace(version='test-stage-guard', repo=REPOSITORY,
                               client_dir=str(release.ROOT / 'client'))
        with self.assertRaisesRegex(ValueError, 'isolated staging'):
            release.build(args)

    def test_release_matches_previously_validated_client(self):
        previous = json.loads((release.ROOT / 'outputs/releases/0.1.2-local/manifest.json').read_text())
        current, _ = validate_release.validate(release.ROOT / 'outputs/releases/0.2.0', REPOSITORY)
        # ZIP timestamps can change when rebuilding; installed client bytes must not.
        payload = lambda manifest: [{key: row[key] for key in ('Path', 'Sha256', 'Size', 'Preserve')}
                                    for row in manifest['Files']]
        self.assertEqual(payload(previous), payload(current))
        self.assertEqual(previous['Base'], current['Base'])

    def test_wrong_publication_destination_is_rejected(self):
        with self.assertRaisesRegex(ValueError, 'destination'):
            validate_release.validate(release.ROOT / 'outputs/releases/0.2.0', 'wrong/repository')

    def test_tampered_manifest_is_rejected_before_upload(self):
        source = release.ROOT / 'outputs/releases/0.2.0'
        with tempfile.TemporaryDirectory(prefix='l2-release-signature-') as folder:
            destination = Path(folder)
            (destination / 'manifest.json').write_bytes((source / 'manifest.json').read_bytes() + b' ')
            (destination / 'manifest.json.sig').write_bytes((source / 'manifest.json.sig').read_bytes())
            with self.assertRaises(Exception):
                validate_release.validate(destination, REPOSITORY)

    def test_published_release_is_never_modified(self):
        with patch.object(publish, 'validate', return_value=({'Version':'0.2.0','Notes':'fixture'}, [])), patch.object(publish, 'GitHub') as client:
            client.return_value.request.side_effect = [
                {'private':False, 'permissions':{'push':True}},
                {'draft':False, 'body':'fixture'}]
            with self.assertRaisesRegex(ValueError, 'already exists'):
                publish.publish(REPOSITORY, '0.2.0', True)
            self.assertTrue(all(len(call.args) == 1 for call in client.return_value.request.call_args_list))

    def test_resumed_draft_requires_matching_asset_digest(self):
        asset = release.ROOT / 'outputs/releases/0.2.0/Launcher.zip'
        self.assertFalse(publish.uploaded_matches({'state':'uploaded','size':asset.stat().st_size,'digest':'sha256:'+'0'*64}, asset))

    def test_draft_without_public_tag_is_resumed(self):
        draft = {'id':123,'tag_name':'0.2.0','draft':True,'body':'fixture',
                 'upload_url':'https://uploads.github.com/example/assets{?name}',
                 'html_url':'https://github.com/example/release'}
        with tempfile.TemporaryDirectory(prefix='l2-publish-test-') as folder, patch.object(publish, 'STATE', Path(folder)), patch.object(publish, 'validate', return_value=({'Version':'0.2.0','Notes':'fixture'}, [])), patch.object(publish, 'sync_history') as history, patch.object(publish, 'GitHub') as client:
            client.return_value.request.side_effect = [
                {'private':False, 'permissions':{'push':True}},
                urllib.error.HTTPError('https://api.github.com/example',404,'Not Found',{},None),
                [draft], [], dict(draft, draft=False)]
            publish.publish(REPOSITORY, '0.2.0', True)
            methods = [call.args[1] for call in client.return_value.request.call_args_list if len(call.args)>1]
            self.assertEqual(methods, ['PATCH'])
            history.assert_called_once_with(REPOSITORY, push=True)


if __name__ == '__main__':
    unittest.main()
