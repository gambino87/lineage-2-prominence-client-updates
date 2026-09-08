"""Record signed public client releases in Git, without storing game binaries."""
import argparse
import json
from pathlib import Path
import re
import subprocess
import urllib.request

from release import STATE
from workspace import SOURCE_ROOT


def download(url):
    request = urllib.request.Request(url, headers={'User-Agent': 'ProminenceReleaseHistory'})
    with urllib.request.urlopen(request, timeout=30) as response:
        if not response.url.startswith('https://'):
            raise ValueError('History download redirected to an insecure URL')
        data = response.read(16 * 1024 * 1024 + 1)
        if len(data) > 16 * 1024 * 1024:
            raise ValueError('Release metadata is too large')
        return data


def file_changes(previous, current):
    before = {row['Path'].lower(): row for row in previous['Files']} if previous else {}
    after = {row['Path'].lower(): row for row in current['Files']}
    changes = []
    for key in sorted(set(before) | set(after)):
        old, new = before.get(key), after.get(key)
        if previous is None and not new.get('Asset'):
            continue
        if old and new and all(old[field] == new[field] for field in ('Sha256', 'Size', 'Preserve')):
            continue
        kind = ('Initial patch' if previous is None else 'Added' if old is None
                else 'No longer managed' if new is None else 'Updated')
        changes.append({
            'Path': (new or old)['Path'], 'Change': kind,
            'BeforeSha256': old['Sha256'] if old else None,
            'AfterSha256': new['Sha256'] if new else None,
            'Size': new['Size'] if new else None,
            'Download': current['AssetBaseUrl'] + new['Asset'] if new and new.get('Asset') else None})
    return changes


def git(*args, check=True):
    result = subprocess.run(['git', *args], cwd=SOURCE_ROOT, capture_output=True, text=True)
    if check and result.returncode:
        raise RuntimeError(result.stderr.strip() or result.stdout.strip() or 'Git operation failed')
    return result


def sync_history(repository, push=False):
    if not re.fullmatch(r'[\w.-]+/[\w.-]+', repository):
        raise ValueError('Repository must be owner/repository')
    expected_remote = f'https://github.com/{repository}'
    if git('remote', 'get-url', 'origin').stdout.strip().removesuffix('.git') != expected_remote:
        raise ValueError('Source checkout origin differs from the release repository')
    if push and git('branch', '--show-current').stdout.strip() != 'main':
        raise ValueError('Publish release history from the main branch')
    releases = []
    page = 1
    while True:
        batch = json.loads(download(f'https://api.github.com/repos/{repository}/releases?per_page=100&page={page}'))
        releases.extend(row for row in batch if not row['draft'])
        if len(batch) < 100:
            break
        page += 1
    releases.sort(key=lambda row: (row['published_at'], row['tag_name']))
    if not releases:
        raise ValueError('No published client releases found')
    paths, records = [], []
    previous = None
    for release in releases:
        version = release['tag_name']
        if not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9._-]{0,60}', version):
            raise ValueError('Unsafe release tag')
        base = f'https://github.com/{repository}/releases/download/{version}/'
        manifest_bytes, signature_bytes = download(base + 'manifest.json'), download(base + 'manifest.json.sig')
        cache = STATE / 'history-cache' / version
        cache.mkdir(parents=True, exist_ok=True)
        manifest_path = cache / 'manifest.json'
        manifest_path.write_bytes(manifest_bytes)
        (cache / 'manifest.json.sig').write_bytes(signature_bytes)
        subprocess.run(['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File',
                        str(Path(__file__).with_name('verify-signature.ps1')), '-Manifest', str(manifest_path),
                        '-PublicKeyFile', str(STATE / 'signing-public.xml')], check=True, capture_output=True)
        manifest = json.loads(manifest_bytes)
        if manifest['Version'] != version or manifest['AssetBaseUrl'] != base:
            raise ValueError('Signed manifest does not match its public release')
        changes = file_changes(previous, manifest)
        record = {'Version': version, 'PreviousVersion': previous['Version'] if previous else None,
                  'Published': release['published_at'], 'Notes': manifest['Notes'],
                  'Release': release['html_url'], 'FilesChanged': changes}
        folder = SOURCE_ROOT / 'client-history' / version
        folder.mkdir(parents=True, exist_ok=True)
        # Preserve the exact signed bytes so the historical manifest remains verifiable.
        for name, data in [('manifest.json', manifest_bytes), ('manifest.json.sig', signature_bytes)]:
            target = folder / name
            if target.exists() and target.read_bytes() != data:
                raise ValueError('Published release metadata changed: ' + version)
            target.write_bytes(data)
        (folder / 'changes.json').write_text(json.dumps(record, indent=2) + '\n', encoding='utf-8')
        comparison = f'Compared with {previous["Version"]}.' if previous else 'Initial cumulative patches relative to the original client.'
        lines = [f'# Client {version}', '', manifest['Notes'], '', comparison, '',
                 f'[Published release]({release["html_url"]}) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)', '',
                 '| Client file | Change | Download |', '| --- | --- | --- |']
        for change in changes:
            link = f'[Patch]({change["Download"]})' if change['Download'] else 'Original archive / no patch'
            lines.append(f'| `{change["Path"]}` | {change["Change"]} | {link} |')
        if not changes:
            lines.append('| — | Client file contents unchanged | — |')
        (folder / 'README.md').write_text('\n'.join(lines) + '\n', encoding='utf-8')
        paths.extend(f'client-history/{version}/{name}' for name in ('manifest.json', 'manifest.json.sig', 'changes.json', 'README.md'))
        records.append(record)
        previous = manifest
    lines = ['# Client release history', '',
             'Generated from verified, signed GitHub Releases. Game binaries remain attached to each release.', '',
             '| Version | Published (UTC) | Client files changed |', '| --- | --- | --- |']
    for record in reversed(records):
        lines.append(f'| [{record["Version"]}]({record["Version"]}/README.md) | {record["Published"]} | {len(record["FilesChanged"])} |')
    (SOURCE_ROOT / 'client-history/README.md').write_text('\n'.join(lines) + '\n', encoding='utf-8')
    paths.append('client-history/README.md')
    if push:
        git('add', '--', *paths)
        changed = git('diff', '--cached', '--quiet', '--', *paths, check=False)
        if changed.returncode == 1:
            git('commit', '--only', '-m', f'docs: record client releases through {records[-1]["Version"]}', '--', *paths)
        elif changed.returncode:
            raise RuntimeError('Could not inspect generated release history')
        git('push', 'origin', 'main')
    print(f'Recorded {len(records)} published client releases in {SOURCE_ROOT / "client-history"}', flush=True)
    return records


if __name__ == '__main__':
    channel = json.loads(Path(__file__).with_name('channel.json').read_text())
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repo', default=channel['Repository'])
    parser.add_argument('--push', action='store_true', help='Commit only generated history files and push main')
    args = parser.parse_args()
    sync_history(args.repo, args.push)
