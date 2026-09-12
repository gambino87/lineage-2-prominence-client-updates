"""Embed channel-specific, signed release notes in the next signed manifest."""
import json
from pathlib import Path
import re
import subprocess


def collect(root, source_root, channel, current_version):
    # Public history contains only confirmed publications. Private candidates
    # live locally and must never be copied into a public notes catalogue.
    base = source_root/'client-history' if channel == 'live' else root/'outputs/releases'
    rows = []
    for path in base.glob('*/manifest.json'):
        manifest = json.loads(path.read_text(encoding='utf-8-sig'))
        version = manifest.get('Version', '')
        is_private = manifest.get('AssetBaseUrl', '').startswith('file:')
        if version == current_version or is_private != (channel == 'test-bench'):
            continue
        if channel == 'test-bench' and not re.fullmatch(r'\d+\.\d+\.\d+', version):
            continue
        subprocess.run(['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File',
                        str(Path(__file__).with_name('verify-signature.ps1')), '-Manifest', str(path),
                        '-PublicKeyFile', str(root/'state/launcher/signing-public.xml')],
                       check=True, capture_output=True)
        rows.append(dict(Version=version, Channel=channel, Published=manifest.get('Published', ''),
                         Notes=manifest.get('Notes', ''), SourceBenchVersion=manifest.get('SourceBenchVersion')))
    return sorted(rows, key=lambda row: (row['Published'], row['Version']), reverse=True)
