"""Freeze a tested bench candidate, then explicitly install/publish/activate it."""
import argparse
from datetime import datetime, timezone
import io
import json
from pathlib import Path
import re
import subprocess
import sys
import tarfile
from types import SimpleNamespace
import zipfile

import release
from publish import GitHub, publish
from server_promotion import inventory, digest
from stage_release import stage
from validate_release import validate
from workspace import ROOT

REPO='gambino87/lineage-2-prominence-client-updates'
TARGET='ubuntu@40.160.140.227'
KEY=Path.home()/'.ssh/l2_ovh_ed25519'
WORKER=Path(__file__).with_name('server_promotion.py')


def ssh(command,capture=False):
    return subprocess.run(['ssh','-i',str(KEY),'-o','BatchMode=yes',TARGET,command],check=True,
                          capture_output=capture,text=capture)


def send_worker():
    remote='/home/ubuntu/prominence-tools'
    ssh('mkdir -p '+remote+' && chmod 700 '+remote)
    # Immutable worker filename prevents another preparation from replacing an in-use tool.
    name='worker-'+digest(WORKER)+'.py'
    subprocess.run(['scp','-i',str(KEY),str(WORKER),TARGET+':'+remote+'/'+name],check=True)
    return remote+'/'+name


def latest(github):
    return github.request(f'https://api.github.com/repos/{REPO}/releases/latest')


def prepare(args):
    folder=ROOT/'state/promotions'/args.version
    if folder.exists():
        raise ValueError('Candidate exists; choose a new version')
    bench, _=validate(args.bench_release)
    if not bench['Version'].startswith('bench-'):
        raise ValueError('Use a tested private bench release')
    previous=latest(GitHub())
    public_source=ROOT/'outputs/releases'/previous['tag_name']
    validate(public_source,REPO)
    folder.mkdir(parents=True)
    worker=send_worker()
    live=json.loads(ssh('sudo -n python3 '+worker+' inspect',capture=True).stdout)
    files=inventory(ROOT/'server')
    metadata=dict(version=args.version,files=files,expected_live=live,
                  bench_version=bench['Version'],test_notes=args.test_notes,
                  migrations='none; migrations require a separate reviewed procedure')
    bundle=folder/'server.tar.gz'
    with tarfile.open(bundle,'w:gz') as archive:
        data=json.dumps(metadata,indent=2).encode()
        info=tarfile.TarInfo('promotion.json');info.size=len(data);info.mode=0o600
        archive.addfile(info,io.BytesIO(data))
        for name, expected in files.items():
            path=ROOT/'server'/name
            if digest(path)!=expected:
                raise ValueError('Bench changed while packaging; use a new candidate')
            archive.add(path,arcname=name,recursive=False)
    # Detect edits during the snapshot, including additions and deletions.
    if inventory(ROOT/'server')!=files:
        raise ValueError('Bench changed while packaging; use a new candidate')
    staged=folder/'client-staging'
    stage(args.bench_release,staged)
    with zipfile.ZipFile(public_source/'Launcher.zip') as archive:
        (folder/'Interlude Launcher.exe').write_bytes(archive.read('Interlude Launcher.exe'))
    release.EXE=folder/'Interlude Launcher.exe'
    release.build(SimpleNamespace(version=args.version,repo=REPO,client_dir=str(staged),host='40.160.140.227',
                                 title='Lineage 2 Prominence',notes=args.notes))
    public,_=validate(ROOT/'outputs/releases'/args.version,REPO)
    signature=lambda manifest:sorted((r['Path'],r['Sha256'],r['Size'],r.get('Preserve',False)) for r in manifest['Files'])
    if signature(bench)!=signature(public):
        raise ValueError('Public client differs from tested bench payload')
    receipt=dict(version=args.version,created=datetime.now(timezone.utc).isoformat(),
                 worker=worker,worker_sha256=digest(WORKER),server_sha256=digest(bundle),
                 client_manifest_sha256=digest(ROOT/'outputs/releases'/args.version/'manifest.json'),
                 previous_public_id=previous['id'],previous_public_tag=previous['tag_name'],
                 test_notes=args.test_notes,bench_version=bench['Version'],
                 changed=[p for p in files if live.get(p)!=files[p]],deleted=sorted(set(live)-set(files)))
    (folder/'candidate.json').write_text(json.dumps(receipt,indent=2))
    (folder/'REVIEW.txt').write_text(f"Candidate {args.version}\nTested client: {bench['Version']}\nTest notes: {args.test_notes}\nChanged server files: {len(receipt['changed'])}\nDeleted server files: {len(receipt['deleted'])}\nLive database and environment configuration are preserved. No SQL migration is included.\n\n"+'\n'.join('CHANGE '+p for p in receipt['changed'])+'\n'+'\n'.join('DELETE '+p for p in receipt['deleted']))
    print('Prepared only; live services and public feed unchanged. Review '+str(folder/'REVIEW.txt'))


def perform(args):
    folder=ROOT/'state/promotions'/args.version
    receipt=json.loads((folder/'candidate.json').read_text())
    public_path=ROOT/'outputs/releases'/args.version
    validate(public_path,REPO)
    if digest(public_path/'manifest.json')!=receipt['client_manifest_sha256']:
        raise ValueError('Client manifest changed after preparation')
    if args.action in {'install','activate','rollback'} and not args.approved:
        raise ValueError('This changes live service. Run only after owner approval, with --approved.')
    worker=receipt['worker']
    # Verify the remote privileged script before each operation.
    actual=ssh('sha256sum '+worker,capture=True).stdout.split()[0]
    if actual!=receipt['worker_sha256']:
        raise ValueError('Remote deployment tool changed')
    github=GitHub()
    if args.action=='install':
        if latest(github)['id']!=receipt['previous_public_id']:
            raise ValueError('Public release changed after preparation')
        bundle=folder/'server.tar.gz'
        if digest(bundle)!=receipt['server_sha256']:
            raise ValueError('Server package changed after preparation')
        remote='/home/ubuntu/prominence-tools/'+args.version+'.tar.gz'
        subprocess.run(['scp','-i',str(KEY),str(bundle),TARGET+':'+remote],check=True)
        ssh(f'sudo -n python3 {worker} install --bundle {remote} --sha256 {receipt["server_sha256"]}')
        print('Maintenance active. Publish the matching client, then activate.')
    elif args.action=='publish':
        if not args.approved:
            raise ValueError('Owner approval required: --approved')
        remote=json.loads(ssh(f'sudo -n cat /opt/prominence/promotions/{args.version}/state.json',capture=True).stdout)
        if remote['status']!='installed-maintenance':
            raise ValueError('Server must be installed and held in maintenance before publication')
        publish(REPO,args.version,True)
    elif args.action=='activate':
        current=latest(github)
        if current['tag_name']!=args.version or current['draft']:
            raise ValueError('Matching client release is not the public latest release')
        manifest=next(a for a in current['assets'] if a['name']=='manifest.json')
        if manifest.get('digest')!='sha256:'+receipt['client_manifest_sha256']:
            raise ValueError('Public manifest differs from the prepared client')
        ssh(f'sudo -n python3 {worker} activate --version {args.version}')
    elif args.action=='rollback':
        remote=json.loads(ssh(f'sudo -n cat /opt/prominence/promotions/{args.version}/state.json',capture=True).stdout)
        if remote['status']=='live':
            raise ValueError('Post-launch database rollback is intentionally blocked; preserve newer player progress.')
        current=latest(github)
        if current['tag_name']==args.version:
            github.request(f'https://api.github.com/repos/{REPO}/releases/{receipt["previous_public_id"]}',
                           'PATCH',{'make_latest':'true'})
            if latest(github)['id']!=receipt['previous_public_id']:
                raise ValueError('Previous client feed could not be restored; maintenance remains active')
        elif current['id']!=receipt['previous_public_id']:
            raise ValueError('Another public release is active; inspect before rollback')
        ssh(f'sudo -n python3 {worker} rollback --version {args.version}')


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action',choices=['prepare','install','publish','activate','rollback'])
    parser.add_argument('--version',required=True)
    parser.add_argument('--bench-release',type=Path)
    parser.add_argument('--test-notes')
    parser.add_argument('--notes',default='Promote the tested test-bench candidate to the hosted server.')
    parser.add_argument('--approved',action='store_true')
    args=parser.parse_args()
    if not re.fullmatch(r'[0-9]+\.[0-9]+\.[0-9]+',args.version):
        parser.error('Use a numeric public version, e.g. 0.2.45')
    if args.action=='prepare' and (not args.bench_release or not args.test_notes):
        parser.error('Preparation requires --bench-release and --test-notes describing completed testing')
    (prepare if args.action=='prepare' else perform)(args)
