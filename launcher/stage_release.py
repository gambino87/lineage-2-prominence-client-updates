"""Create cumulative client staging from a previously validated local release."""
import argparse
from pathlib import Path
import zipfile

from release import ROOT
from validate_release import validate


def stage(source, output):
    source, output = Path(source).resolve(), Path(output).resolve()
    live = (ROOT / 'client').resolve()
    if output == live or live in output.parents or output in live.parents:
        raise ValueError('Staging must be separate from the live client')
    if output.exists():
        raise ValueError('Choose a new staging directory')
    manifest, _ = validate(source)
    output.mkdir(parents=True)
    count = 0
    for row in manifest['Files']:
        if row.get('Asset'):
            with zipfile.ZipFile(source / row['Asset']) as archive:
                archive.extract(row['Path'], output)
            count += 1
    print(f'Staged {count} cumulative patches from {manifest["Version"]} in {output}')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--from-release', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    stage(args.from_release, args.output)
