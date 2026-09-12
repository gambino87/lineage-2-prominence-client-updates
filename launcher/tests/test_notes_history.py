import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from notes_history import collect


class NotesHistoryTests(unittest.TestCase):
    def test_channels_publication_and_order(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            source = root/'source'
            def write(base, version, url, date):
                folder = base/version
                folder.mkdir(parents=True)
                (folder/'manifest.json').write_text(json.dumps(dict(Version=version,
                    AssetBaseUrl=url, Notes=version, Published=date)))
            write(root/'outputs/releases', '0.2.9', 'file:///private/', '2026-01-01')
            write(root/'outputs/releases', '0.2.10', 'file:///private/', '2026-01-02')
            write(root/'outputs/releases', 'alpha-0.0.9', 'https://public/', '2026-01-03')
            write(source/'client-history', 'alpha-0.0.8', 'https://public/', '2026-01-01')
            write(source/'client-history', '0.2.99', 'file:///private/', '2026-01-03')
            with patch('notes_history.subprocess.run') as verify:
                self.assertEqual([r['Version'] for r in collect(root,source,'test-bench','0.2.11')], ['0.2.10','0.2.9'])
                self.assertEqual([r['Version'] for r in collect(root,source,'live','alpha-0.0.10')], ['alpha-0.0.8'])
                self.assertEqual(verify.call_count,3)
            with patch('notes_history.subprocess.run',side_effect=subprocess.CalledProcessError(1,'verify')):
                with self.assertRaises(subprocess.CalledProcessError):
                    collect(root,source,'test-bench','0.2.11')


if __name__ == '__main__':
    unittest.main()
