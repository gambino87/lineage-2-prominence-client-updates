"""Private server promotion worker. Inspect is read-only; other actions require root."""
import argparse
import gzip
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import shutil
import socket
import subprocess
import tarfile
import time

SERVER=Path('/opt/prominence/server')
HISTORY=Path('/opt/prominence/promotions')
MAINTENANCE=Path('/opt/prominence/maintenance')
ENVIRONMENT={'database.ini','server.ini','ipconfig.xml','default-ipconfig.xml','hexid.txt','interface.ini'}


def managed(name):
    p=PurePosixPath(name)
    if p.is_absolute() or '..' in p.parts or '\\' in name or ':' in name:
        return False
    if p.suffix.lower() in {'.sql','.log','.bak','.tmp','.key','.pem'}:
        return False
    if name.startswith('libs/'):
        return len(p.parts)==2 and p.suffix=='.jar'
    if name.startswith('game/config/'):
        return p.name.lower() not in ENVIRONMENT
    return name.startswith(('game/data/','login/data/'))


def digest(path):
    with open(path,'rb') as f:
        return hashlib.file_digest(f,'sha256').hexdigest()


def inventory(root):
    result={}
    for base, dirs, files in os.walk(root,followlinks=False):
        dirs[:]=[d for d in dirs if not (Path(base)/d).is_symlink()]
        for name in files:
            path=Path(base)/name
            relative=path.relative_to(root).as_posix()
            if managed(relative) and not path.is_symlink():
                result[relative]=digest(path)
    return dict(sorted(result.items()))


def command(*args,**kwargs):
    return subprocess.run(args,check=True,**kwargs)


def save(folder,state):
    temporary=folder/'state.json.new'
    temporary.write_text(json.dumps(state,indent=2))
    temporary.replace(folder/'state.json')


def gate(close):
    rule=['!','-i','lo','-p','tcp','-m','multiport','--dports','2106,7777','-m','comment','--comment','prominence-promotion','-j','REJECT']
    for tool in ['iptables','ip6tables']:
        present=subprocess.run([tool,'-C','INPUT',*rule],capture_output=True).returncode==0
        if close and not present:
            command(tool,'-I','INPUT','1',*rule)
        elif not close and present:
            command(tool,'-D','INPUT',*rule)


def begin_maintenance():
    # Reapply the gate before either server can start following a VPS reboot.
    worker=Path('/opt/prominence/maintenance-worker.py')
    shutil.copyfile(__file__,worker)
    worker.chmod(0o600)
    unit=Path('/etc/systemd/system/prominence-maintenance.service')
    unit.write_text('[Unit]\nDescription=Prominence maintenance gate\nBefore=prominence-login.service prominence-game.service\nAfter=ufw.service\n\n[Service]\nType=oneshot\nExecStart=/usr/bin/python3 /opt/prominence/maintenance-worker.py boot-gate\nRemainAfterExit=yes\n')
    for service in ['prominence-login','prominence-game']:
        dropin=Path('/etc/systemd/system')/(service+'.service.d')
        dropin.mkdir(exist_ok=True)
        (dropin/'maintenance.conf').write_text('[Unit]\nRequires=prominence-maintenance.service\nAfter=prominence-maintenance.service\n')
    MAINTENANCE.touch(mode=0o600)
    command('systemctl','daemon-reload')
    gate(True)


def reopen(folder,state,status):
    # Record admission BEFORE opening either IP family. Never restore a DB after this point.
    state['status']=status
    save(folder,state)
    gate(False)
    MAINTENANCE.unlink(missing_ok=True)
    print(json.dumps(state))


def check_bundle(bundle, expected):
    if digest(bundle)!=expected:
        raise ValueError('Server package checksum differs from the frozen candidate')
    with tarfile.open(bundle,'r:gz') as archive:
        members=archive.getmembers()
        names=[m.name for m in members]
        if len(names)!=len(set(names)) or 'promotion.json' not in names:
            raise ValueError('Invalid package inventory')
        metadata=json.load(archive.extractfile('promotion.json'))
        if not all(managed(name) for name in metadata['expected_live']):
            raise ValueError('Unsafe baseline path')
        if set(names)!={'promotion.json',*metadata['files']}:
            raise ValueError('Package has unlisted files')
        for item in members:
            if not item.isfile() or (item.name!='promotion.json' and not managed(item.name)):
                raise ValueError('Unsafe or environment-specific package path: '+item.name)
            if item.name!='promotion.json':
                with archive.extractfile(item) as f:
                    if hashlib.file_digest(f,'sha256').hexdigest()!=metadata['files'][item.name]:
                        raise ValueError('Corrupt payload: '+item.name)
    return metadata


def stop(require_clean=True):
    # Stop game first while its login link and database are still available.
    command('systemctl','stop','prominence-game')
    command('systemctl','stop','prominence-login')
    for unit in ['prominence-game','prominence-login']:
        props=command('systemctl','show',unit,'--property=MainPID','--property=Result',capture_output=True,text=True).stdout
        if 'MainPID=0' not in props or (require_clean and 'Result=success' not in props):
            raise RuntimeError('Service did not stop cleanly; deployment aborted: '+unit)


def backup(folder):
    target=folder/'database.sql.gz.partial'
    with open(folder/'dump-errors.txt','wb') as errors, gzip.open(target,'wb') as output:
        process=subprocess.Popen(['mariadb-dump','--single-transaction','--routines','--events','--hex-blob','l2jmobiusinterlude'],stdout=subprocess.PIPE,stderr=errors)
        shutil.copyfileobj(process.stdout,output)
        if process.wait()!=0:
            raise RuntimeError('Database backup failed; inspect dump-errors.txt')
    with gzip.open(target,'rb') as source:
        while source.read(1024*1024):
            pass
    target.replace(folder/'database.sql.gz')


def health():
    until=time.monotonic()+180
    while time.monotonic()<until:
        try:
            for port in [2106,7777]:
                with socket.create_connection(('127.0.0.1',port),2):
                    pass
            pid=command('systemctl','show','prominence-game','--property=MainPID','--value',capture_output=True,text=True).stdout.strip()
            log=command('journalctl',f'_PID={pid}','--no-pager',capture_output=True,text=True).stdout
            if 'Registered on login as Server' in log and 'GameServer: Started' in log:
                return
        except (OSError,subprocess.CalledProcessError):
            pass
        time.sleep(2)
    raise RuntimeError('Startup verification failed; maintenance remains active. Run rollback.')


def install(bundle,sha):
    metadata=check_bundle(bundle,sha)
    version=metadata['version']
    if not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9._-]{0,60}',version):
        raise ValueError('Invalid version')
    folder=HISTORY/version
    for other in HISTORY.glob('*/state.json'):
        if json.loads(other.read_text())['status'] not in {'live','rolled-back'}:
            raise ValueError('An earlier promotion is unfinished: '+str(other))
    if folder.exists():
        raise ValueError('Promotion already attempted; inspect its state before retrying')
    if inventory(SERVER)!=metadata['expected_live']:
        raise ValueError('Live files changed after preparation. Prepare a fresh candidate.')
    # No player data comes from test SQL.
    folder.mkdir(parents=True,mode=0o700)
    state=dict(version=version,status='prepared',package_sha256=sha)
    save(folder,state)
    begin_maintenance()
    try:
        stop()
        if inventory(SERVER)!=metadata['expected_live']:
            raise ValueError('Live managed files changed during shutdown; inspect before retrying')
        backup(folder)
        candidate=folder/'candidate'
        shutil.copytree(SERVER,candidate,symlinks=True)
        for old in set(metadata['expected_live'])-set(metadata['files']):
            target=candidate/old
            if target.is_symlink() or not target.resolve().is_relative_to(candidate.resolve()):
                raise ValueError('Unsafe deletion target')
            target.unlink()
        with tarfile.open(bundle,'r:gz') as archive:
            for name in metadata['files']:
                target=candidate/name
                if target.is_symlink() or not target.resolve().is_relative_to(candidate.resolve()):
                    raise ValueError('Unsafe write target')
                target.parent.mkdir(parents=True,exist_ok=True)
                with archive.extractfile(name) as source, target.open('wb') as output:
                    shutil.copyfileobj(source,output)
                target.chmod(0o640)
        if inventory(candidate)!=metadata['files']:
            raise ValueError('Candidate verification failed')
        custom=candidate/'game/config/custom'
        if not custom.exists() and (candidate/'game/config/Custom').is_dir():
            custom.symlink_to('Custom',target_is_directory=True)
        command('chown','-R','prominence:prominence',str(candidate))
        state['status']='swapping'
        save(folder,state)
        SERVER.rename(folder/'previous-server')
        candidate.rename(SERVER)
        state['status']='installed-maintenance'
        save(folder,state)
        print(json.dumps(state))
    except Exception:
        # A swap interrupted between renames is recoverable from previous-server.
        if not SERVER.exists() and (folder/'previous-server').exists():
            (folder/'previous-server').rename(SERVER)
        state['status']='failed-maintenance'
        save(folder,state)
        raise


def activate(version):
    folder=HISTORY/version
    state=json.loads((folder/'state.json').read_text())
    if state['status']=='live':
        # Safe retry if the final firewall operation was interrupted.
        reopen(folder,state,'live')
        return
    if state['status']!='installed-maintenance':
        raise ValueError('Candidate is not waiting for activation')
    # Do not silently reopen an interrupted or explicitly rolled-back deployment.
    gate(True)
    command('systemctl','start','prominence-login','prominence-game')
    health()
    reopen(folder,state,'live')


def rollback(version):
    folder=HISTORY/version
    state=json.loads((folder/'state.json').read_text())
    if state['status']=='live':
        raise ValueError('Players may have new progress. Post-launch rollback requires a separate reviewed promotion; database restore refused.')
    if state['status']=='rolled-back':
        reopen(folder,state,'rolled-back')
        return
    if state['status'] not in {'prepared','swapping','failed-maintenance','installed-maintenance'}:
        raise ValueError('Unrecognized promotion state; recovery requires inspection')
    gate(True)
    stop(require_clean=False)
    old=folder/'previous-server'
    if old.exists():
        if SERVER.exists():
            SERVER.rename(folder/'rejected-server')
        old.rename(SERVER)
    if (folder/'database.sql.gz').exists():
        # No players have been admitted during maintenance. Undo startup-only DB changes.
        with gzip.open(folder/'database.sql.gz','rb') as source:
            command('mariadb','l2jmobiusinterlude',input=source.read())
    command('systemctl','start','prominence-login','prominence-game')
    health()
    reopen(folder,state,'rolled-back')


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action',choices=['inspect','install','activate','rollback','boot-gate'])
    parser.add_argument('--bundle',type=Path)
    parser.add_argument('--sha256')
    parser.add_argument('--version')
    args=parser.parse_args()
    if os.geteuid()!=0:
        parser.error('Run via sudo')
    if args.action=='boot-gate':
        if MAINTENANCE.exists():
            gate(True)
        raise SystemExit(0)
    import fcntl
    lock=open('/run/prominence-promotion.lock','w')
    fcntl.flock(lock,fcntl.LOCK_EX | fcntl.LOCK_NB)
    if args.action=='inspect':
        print(json.dumps(inventory(SERVER)))
    elif args.action=='install':
        install(args.bundle,args.sha256)
    else:
        if not args.version or not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9._-]{0,60}',args.version):
            parser.error('Valid --version required')
        {'activate':activate,'rollback':rollback}[args.action](args.version)
