$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'workspace.ps1')
$testRoot = Join-Path $workspace ('state/keybind-update-builds/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $testRoot | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$sources = (Get-ChildItem (Join-Path $PSScriptRoot 'keybinds') -Filter '*.cs').FullName
$testExe = Join-Path $testRoot 'Tests.exe'
& $compiler /nologo /target:exe /main:KeybindUpdateTests /out:$testExe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.Numerics.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll (Join-Path $PSScriptRoot 'Core.cs') (Join-Path $PSScriptRoot 'tests/KeybindUpdateTests.cs') @sources
if ($LASTEXITCODE -ne 0) { throw 'Keybind update test compilation failed' }
& $testExe $workspace
if ($LASTEXITCODE -ne 0) { throw 'Keybind update tests failed' }
