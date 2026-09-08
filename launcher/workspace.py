"""Locate build inputs separately from the tracked launcher source checkout."""
import os
from pathlib import Path

SOURCE_ROOT = Path(__file__).resolve().parents[1]
if os.environ.get('PROMINENCE_WORKSPACE'):
    ROOT = Path(os.environ['PROMINENCE_WORKSPACE']).resolve()
elif SOURCE_ROOT.parent.name == 'publishing' and (SOURCE_ROOT.parent.parent / 'downloads/client-manifest.json').is_file():
    ROOT = SOURCE_ROOT.parent.parent
else:
    ROOT = SOURCE_ROOT
