"""Verify a complete release against the established public signing key."""
import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import subprocess
import zipfile

from release import ROOT, STATE, sha, usable


def validate(folder, repository=None):
    folder = Path(folder).resolve()
    manifest_path = folder / 'manifest.json'
    subprocess.run(['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File',
                    str(Path(__file__).with_name('verify-signature.ps1')),
                    '-Manifest', str(manifest_path), '-PublicKeyFile',
                    str(STATE / 'signing-public.xml')], check=True, capture_output=True)
    manifest = json.loads(manifest_path.read_text(encoding='utf-8-sig'))
    if manifest['Schema'] != 1 or not manifest['Files']:
        raise ValueError('Unsupported or empty manifest')
    if repository:
        expected = f'https://github.com/{repository}/releases/download/{manifest["Version"]}/'
        if manifest['AssetBaseUrl'] != expected:
            raise ValueError('Manifest repository or release tag does not match publication destination')
    assets = [folder / name for name in ('manifest.json', 'manifest.json.sig', 'Launcher.zip')]
    seen = set()
    for row in manifest['Files']:
        path = row['Path']
        if '\\' in path or ':' in path or path.startswith('/') or any(p in ('', '.', '..') for p in path.split('/')) or not usable(path):
            raise ValueError('Unsafe client path: ' + path)
        if path.lower() in seen:
            raise ValueError('Duplicate client path: ' + path)
        seen.add(path.lower())
        if not row.get('Asset'):
            continue
        name = row['Asset']
        if PurePosixPath(name).name != name or '\\' in name or ':' in name:
            raise ValueError('Unsafe release asset')
        asset = folder / name
        if asset.stat().st_size != row['AssetSize'] or sha(asset) != row['AssetSha256']:
            raise ValueError('Patch checksum failed: ' + path)
        with zipfile.ZipFile(asset) as archive:
            if archive.namelist() != [path] or archive.getinfo(path).file_size != row['Size']:
                raise ValueError('Unexpected patch content: ' + path)
            with archive.open(path) as entry:
                if hashlib.file_digest(entry, 'sha256').hexdigest() != row['Sha256']:
                    raise ValueError('Client checksum failed: ' + path)
        assets.append(asset)
    if not {'system/l2.exe', 'system/psetup.dll'} <= seen:
        raise ValueError('Missing client essentials')
    with zipfile.ZipFile(folder / 'Launcher.zip') as archive:
        if set(archive.namelist()) != {'Interlude Launcher.exe', 'launcher.json', 'START HERE.txt'}:
            raise ValueError('Unexpected launcher package content')
        config = json.loads(archive.read('launcher.json'))
        if config['PublicKey'].strip() != (STATE / 'signing-public.xml').read_text().strip():
            raise ValueError('Launcher public key differs from the established signing key')
        if repository and (config['Feed'] != f'https://github.com/{repository}/releases/latest/download/manifest.json'
                           or config['AllowLocalFeed'] or config['ClientDirectory']):
            raise ValueError('Launcher package is not configured for the public update feed')
    if len(assets) >= 1000 or any(p.stat().st_size >= 2 * 1024**3 for p in assets):
        raise ValueError('Release exceeds asset count or size limits')
    return manifest, list(dict.fromkeys(assets))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('release_directory', type=Path)
    parser.add_argument('--repo')
    args = parser.parse_args()
    manifest, assets = validate(args.release_directory, args.repo)
    print(f'Validated {manifest["Version"]}: signature, {len(manifest["Files"])} client files, {len(assets)} release assets.')
