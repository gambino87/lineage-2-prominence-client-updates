"""Build a signed, cumulative client release. Does not publish or touch live files."""
from pathlib import Path
import argparse, hashlib, json, re, subprocess, zipfile

from workspace import ROOT
STATE = ROOT / 'state/launcher'
BASE = ROOT / 'downloads/Lineage II Mobius Interlude.zip'
EXE = ROOT / 'outputs/launcher/Interlude Launcher.exe'
ALLOWED = {'animations','forcefeedback','l2text','maps','music','sounds','staticmeshes','system','systextures','textures','voice'}

def sha(path):
    with path.open('rb') as f:
        return hashlib.file_digest(f, 'sha256').hexdigest()

def usable(path):
    low = path.lower()
    return '/' in low and low.split('/')[0] in ALLOWED and not any(
        marker in low for marker in ('interface_pre_', '.bak', '.log', '.tmp', 'compiledshader', 'running.ini', 's_info.ini'))

def json_write(path, data):
    path.write_text(json.dumps(data, indent=2), encoding='utf-8')

def baseline():
    STATE.mkdir(parents=True, exist_ok=True)
    provenance = json.loads((ROOT / 'downloads/client-manifest.json').read_text())
    cache = STATE / 'base-index.json'
    # Verify the archive on every release; cached entry hashes only save decompression.
    print('Verifying base archive…', flush=True)
    assert BASE.stat().st_size == provenance['size'] and sha(BASE) == provenance['sha256'], 'Wrong base archive'
    if cache.exists():
        data = json.loads(cache.read_text())
        if data['archive_sha256'] == provenance['sha256']:
            return provenance, data['files']
    rows = []
    with zipfile.ZipFile(BASE) as z:
        entries = [i for i in z.infolist() if not i.is_dir() and usable(i.filename)]
        for n, entry in enumerate(entries):
            with z.open(entry) as f:
                digest = hashlib.file_digest(f, 'sha256').hexdigest()
            rows.append(dict(Path=entry.filename, Sha256=digest, Size=entry.file_size))
            if n % 200 == 0:
                print(f'Indexed {n}/{len(entries)} base files', flush=True)
    json_write(cache, dict(archive_sha256=provenance['sha256'], files=rows))
    return provenance, rows

def sign_launcher(folder, executable, channel):
    manifest=json.loads((folder/'manifest.json').read_text(encoding='utf-8'))
    manifest['Launcher']=dict(Version='1.1.1',Channel=channel,Sha256=sha(folder/'Launcher.zip'),
                              Size=(folder/'Launcher.zip').stat().st_size,ExeSha256=sha(executable))
    json_write(folder/'manifest.json',manifest)
    subprocess.run([str(executable),'--sign',str(STATE/'signing-private.xml'),str(folder/'manifest.json')],check=True)


def build(args):
    if not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9._-]{0,60}', args.version):
        raise ValueError('Use a simple version label, e.g. 0.1.0')
    if args.repo and not re.fullmatch(r'[\w.-]+/[\w.-]+', args.repo):
        raise ValueError('Repository must be owner/repository')
    client = Path(args.client_dir).resolve()
    live = (ROOT / 'client').resolve()
    if not client.is_dir() or client == live or live in client.parents or client in live.parents:
        raise ValueError('--client-dir must be an isolated staging directory, outside the live client')
    out = ROOT / 'outputs/releases' / args.version
    if out.exists():
        raise ValueError('Release already exists. Use a new version; published releases are immutable.')
    provenance, base = baseline()
    out.mkdir(parents=True)
    private, public = STATE / 'signing-private.xml', STATE / 'signing-public.xml'
    if not private.exists():
        subprocess.run([str(EXE), '--create-key', str(private), str(public)], check=True)
    current = {p.relative_to(client).as_posix().lower(): p for p in client.rglob('*') if p.is_file()}
    rows, assets = [], []
    originals = {r['Path'].lower(): r for r in base}
    for key in sorted(set(originals) | {k for k in current if usable(k)}):
        original = originals.get(key)
        file = current.get(key)
        preserve = key.endswith('.ini')
        # Settings default to the pristine client; never distribute the developer's personal INIs.
        if preserve and original:
            rows.append(dict(original, Preserve=True))
            continue
        if file is None:
            if original: rows.append(dict(original, Preserve=False))
            continue
        digest, size = sha(file), file.stat().st_size
        rel = original['Path'] if original else file.relative_to(client).as_posix()
        row = dict(Path=rel, Sha256=digest, Size=size, Preserve=preserve)
        if original is None or digest != original['Sha256']:
            asset = hashlib.sha256((rel+'\0'+digest).encode()).hexdigest() + '.zip'
            with zipfile.ZipFile(out/asset, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
                z.write(file, rel)
            with zipfile.ZipFile(out/asset) as z, z.open(rel) as entry:
                assert hashlib.file_digest(entry, 'sha256').hexdigest() == digest, 'Client changed during packaging; rebuild with a new version'
            assert (out/asset).stat().st_size < 2*1024**3, 'Release asset exceeds GitHub limit'
            row.update(Asset=asset, AssetSha256=sha(out/asset), AssetSize=(out/asset).stat().st_size)
            assets.append(dict(path=rel, bytes=size, download_bytes=row['AssetSize']))
        rows.append(row)
    from datetime import datetime, timezone
    manifest = dict(Schema=1, Version=args.version, Published=datetime.now(timezone.utc).isoformat(),
                    AssetBaseUrl=f'https://github.com/{args.repo}/releases/download/{args.version}/' if args.repo else out.as_uri()+'/',
                    Notes=args.notes, Base=dict(Url=provenance['url'], Sha256=provenance['sha256'], Size=provenance['size']), Files=rows)
    json_write(out/'manifest.json', manifest)
    subprocess.run([str(EXE), '--sign', str(private), str(out/'manifest.json')], check=True)
    assert len(assets)+3 < 1000, 'Too many GitHub release assets'
    config = dict(Title=args.title, Feed=f'https://github.com/{args.repo}/releases/latest/download/manifest.json' if args.repo else (out/'manifest.json').as_uri(),
                  PublicKey=public.read_text(), AllowLocalFeed=not bool(args.repo),
                  ClientDirectory='' if args.repo else str(ROOT/'client'), ServerAddress=args.host)
    # The installed launcher switches feeds only after publication succeeds.
    json_write(out/'launcher.json', config)
    with zipfile.ZipFile(out/'Launcher.zip', 'w', zipfile.ZIP_DEFLATED) as z:
        z.write(EXE, EXE.name)
        z.writestr('launcher.json', json.dumps(config, indent=2))
        z.writestr('START HERE.txt', 'Extract this folder, then open Interlude Launcher.exe. Choose a client folder and wait for the check. Click Install for a new client or Update when changes are available. Up to date is disabled. You can select the supported downloaded ZIP to avoid downloading it again.\r\n')
    sign_launcher(out,EXE,'live' if args.repo else 'test-bench')
    json_write(STATE/f'release-{args.version}.json', dict(version=args.version, repository=args.repo, client_staging=str(client), modified=assets, files=len(rows), download_bytes=sum(a['download_bytes'] for a in assets), local_only=not bool(args.repo)))
    print(f'Release {args.version}: {len(rows)} client files, {len(assets)} patches, {sum(a["download_bytes"] for a in assets)/1048576:.1f} MiB download', flush=True)
    print(out)

if __name__ == '__main__':
    channel_path = Path(__file__).with_name('channel.json')
    channel = json.loads(channel_path.read_text()) if channel_path.exists() else {}
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--version', required=True)
    destination = p.add_mutually_exclusive_group()
    destination.add_argument('--repo', default=channel.get('Repository', ''))
    destination.add_argument('--local', action='store_true', help='Build an explicit file:// development feed')
    p.add_argument('--client-dir', required=True, help='Cumulative staged client files; omitted files use the pristine base archive')
    p.add_argument('--host', default=channel.get('ServerAddress', '127.0.0.1'))
    p.add_argument('--title', default=channel.get('Title', 'Interlude Test Launcher'))
    p.add_argument('--notes', default='Initial playtest client: custom jobs, combat skills, crafting, Hearthstones, and interface changes.')
    args = p.parse_args()
    if args.local:
        args.repo = ''
    build(args)
