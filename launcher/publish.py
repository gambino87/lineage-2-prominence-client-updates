"""Upload verified release assets using the existing GitHub login. Draft by default."""
import argparse
import json
import os
from pathlib import Path
import re
import subprocess
import urllib.error
import urllib.parse
import urllib.request

from release import ROOT, STATE, json_write, sha
from validate_release import validate
from workspace import SOURCE_ROOT
from sync_history import sync_history


class GitHub:
    def __init__(self):
        self.token = os.environ.get('GH_TOKEN') or os.environ.get('GITHUB_TOKEN')
        if not self.token:
            credentials = subprocess.run(
                ['git', 'credential', 'fill'], input='protocol=https\nhost=github.com\n\n',
                text=True, capture_output=True, cwd=SOURCE_ROOT,
                env={**os.environ, 'GIT_TERMINAL_PROMPT': '0', 'GCM_INTERACTIVE': 'never'})
            fields = dict(line.split('=', 1) for line in credentials.stdout.splitlines() if '=' in line)
            if credentials.returncode == 0:
                self.token = fields.get('password')
        if not self.token:
            raise RuntimeError('Sign into GitHub through Git Credential Manager, or provide GH_TOKEN in the environment.')

    def request(self, url, method='GET', data=None, content_type='application/json'):
        # Credentials are used only for GitHub API hosts, never included in release assets.
        address = urllib.parse.urlparse(url)
        if address.scheme != 'https' or address.netloc not in {'api.github.com', 'uploads.github.com'}:
            raise ValueError('Unexpected GitHub API address')
        if isinstance(data, dict):
            data = json.dumps(data).encode('utf-8')
        headers = {'Authorization': 'Bearer ' + self.token, 'Accept': 'application/vnd.github+json',
                   'User-Agent': 'InterludeReleaseBuilder', 'X-GitHub-Api-Version': '2026-03-10'}
        if data is not None:
            headers['Content-Type'] = content_type
        class NoRedirect(urllib.request.HTTPRedirectHandler):
            def redirect_request(self, req, fp, code, msg, headers, newurl):
                return None
        request = urllib.request.Request(url, data=data, headers=headers, method=method)
        with urllib.request.build_opener(NoRedirect()).open(request, timeout=180) as response:
            return json.load(response)


def uploaded_matches(remote, local):
    return (remote['state'] == 'uploaded' and remote['size'] == local.stat().st_size
            and remote.get('digest') == 'sha256:' + sha(local))


def publish(repository, version, make_public=False):
    if not re.fullmatch(r'[\w.-]+/[\w.-]+', repository) or not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9._-]{0,60}', version):
        raise ValueError('Invalid repository or version')
    manifest, assets = validate(ROOT / 'outputs/releases' / version, repository)
    if manifest['Version'] != version:
        raise ValueError('Release directory and manifest version differ')
    print(f'Validated {len(assets)} signed release assets.', flush=True)
    github = GitHub()
    api = f'https://api.github.com/repos/{repository}'
    repo = github.request(api)
    if repo['private'] or not repo.get('permissions', {}).get('push'):
        raise ValueError('The update feed needs a public repository with publishing access')
    try:
        release = github.request(api + '/releases/tags/' + version)
    except urllib.error.HTTPError as error:
        if error.code != 404:
            raise
        release = None
        # GitHub's tag endpoint excludes unpublished drafts. Find them via the authenticated list.
        page = 1
        matches = []
        while True:
            batch = github.request(api + f'/releases?per_page=100&page={page}')
            matches.extend(item for item in batch if item['tag_name'] == version)
            if len(batch) < 100:
                break
            page += 1
        if len(matches) > 1:
            raise ValueError('Multiple releases use this tag. Review them before retrying.')
        if matches:
            release = matches[0]
    # Drafts can be resumed after an interrupted upload, but existing public releases are immutable.
    if release is not None and (not release['draft'] or release['body'] != manifest['Notes']):
        raise ValueError('Release already exists or draft notes differ. Use a new version.')
    if release is None:
        release = github.request(api + '/releases', 'POST', {
            'tag_name': version, 'target_commitish': repo['default_branch'],
            'name': 'Client ' + version, 'body': manifest['Notes'], 'draft': True,
            'prerelease': False})
    release_api = api + '/releases/' + str(release['id'])
    existing = {}
    page = 1
    while True:
        batch = github.request(release_api + f'/assets?per_page=100&page={page}')
        existing.update({item['name']: item for item in batch})
        if len(batch) < 100:
            break
        page += 1
    if set(existing) - {asset.name for asset in assets}:
        raise ValueError('Draft has unexpected assets. Review it before retrying.')
    upload = release['upload_url'].split('{')[0]
    for number, asset in enumerate(assets, 1):
        remote = existing.get(asset.name)
        if remote is None:
            print(f'Uploading {number}/{len(assets)}: {asset.name}', flush=True)
            remote = github.request(upload + '?' + urllib.parse.urlencode({'name': asset.name}),
                                    'POST', asset.read_bytes(), 'application/octet-stream')
        if not uploaded_matches(remote, asset):
            raise ValueError('Uploaded asset checksum differs: ' + asset.name)
    if make_public:
        release = github.request(release_api, 'PATCH', {'draft': False, 'make_latest': 'true'})
    json_write(STATE / f'publication-{version}.json', {
        'repository': repository, 'version': version, 'release_id': release['id'],
        'url': release['html_url'], 'published': not release['draft'],
        'verified_assets': len(assets)})
    print(('Published: ' if not release['draft'] else 'Draft ready: ') + release['html_url'])
    if not release['draft']:
        try:
            sync_history(repository, push=True)
        except Exception as error:
            raise RuntimeError('Release is published, but Git history sync failed. Run python launcher/sync_history.py --push to retry.') from error


if __name__ == '__main__':
    channel = json.loads(Path(__file__).with_name('channel.json').read_text())
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repo', default=channel['Repository'])
    parser.add_argument('--version', required=True)
    parser.add_argument('--publish', action='store_true')
    args = parser.parse_args()
    publish(args.repo, args.version, args.publish)
