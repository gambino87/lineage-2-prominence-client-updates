# Locate build inputs from either the source checkout or the workspace launcher junction.
if ($env:PROMINENCE_WORKSPACE) {
 $workspace = [IO.Path]::GetFullPath($env:PROMINENCE_WORKSPACE)
} else {
 $workspace = Split-Path $PSScriptRoot -Parent
 $container = Split-Path $workspace -Parent
 $candidate = Split-Path $container -Parent
 if ((Split-Path $container -Leaf) -eq 'publishing' -and (Test-Path -LiteralPath (Join-Path $candidate 'downloads/client-manifest.json'))) {
  $workspace = $candidate
 }
}
