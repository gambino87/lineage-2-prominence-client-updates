"""Prepare a signed private client feed and the owner's local-only launcher."""
import argparse
import json
import re
from pathlib import Path
import subprocess
import zipfile
from types import SimpleNamespace

import release
from stage_release import stage
from validate_release import validate
from workspace import ROOT


def prepare(version, source, client_dir=None, notes='Private test-bench candidate.'):
    if not re.fullmatch(r'[0-9]+\.[0-9]+\.[0-9]+',version):
        raise ValueError('Use a numeric test-bench version such as 0.2.45')
    if (ROOT/'outputs/releases'/version).exists():
        raise ValueError('Private release already exists; choose a new version')
    output = ROOT/'outputs/test-bench'
    output.mkdir(parents=True, exist_ok=True)
    subprocess.run(['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File',
                    str(Path(__file__).with_name('build.ps1')),'-OutputDirectory',str(output),'-TestBench'],check=True)
    if client_dir is None:
        client_dir=ROOT/'state/launcher-staging'/version
        stage(source, client_dir)
    # Building a candidate never installs its client files.
    release.EXE=output/'Interlude Launcher.exe'
    release.build(SimpleNamespace(version=version,repo='',client_dir=str(client_dir),
                                 host='127.0.0.1',title='Prominence — TEST BENCH',notes=notes))
    folder=ROOT/'outputs/releases'/version
    config=json.loads((folder/'launcher.json').read_text())
    config['ClientDirectory']=str(output/'client')
    config['Feed']=(folder/'manifest.json').as_uri()
    (folder/'launcher.json').write_text(json.dumps(config,indent=2))
    with zipfile.ZipFile(folder/'Launcher.zip','w',zipfile.ZIP_DEFLATED) as z:
        z.write(release.EXE,release.EXE.name)
        z.writestr('launcher.json',json.dumps(config,indent=2))
        z.writestr('START HERE.txt','Private test bench. Start Test Bench.cmd; apply updates yourself. Never distribute this package.\r\n')
    validate(folder)
    config['Feed']=(output/'manifest.json').as_uri()
    # Signed manifest references immutable per-version assets; pointer moves only after validation.
    for name in ['manifest.json','manifest.json.sig']:
        temporary=output/(name+'.new')
        temporary.write_bytes((folder/name).read_bytes())
        temporary.replace(output/name)
    (output/'launcher.json').write_text(json.dumps(config,indent=2))
    receipt=dict(version=version,release=str(folder),client=str(output/'client'),staging=str(client_dir))
    (output/'candidate.json').write_text(json.dumps(receipt,indent=2))
    print('Private candidate available. Close Lineage II and click Install or Update in the TEST BENCH launcher.')


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--version',required=True)
    parser.add_argument('--from-release',type=Path)
    parser.add_argument('--client-dir',type=Path)
    parser.add_argument('--notes',default='Private test-bench candidate.')
    args=parser.parse_args()
    if not args.from_release and not args.client_dir:
        parser.error('Provide --from-release or --client-dir')
    prepare(args.version,args.from_release,args.client_dir,args.notes)
