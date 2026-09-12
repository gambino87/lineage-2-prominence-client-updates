$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'workspace.ps1')
$testRoot = Join-Path $workspace ('state/launcher-ui-tests/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $testRoot | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$testExe = Join-Path $testRoot 'WindowState.exe'
& $compiler /nologo /target:exe /main:WindowState /out:$testExe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll (Join-Path $PSScriptRoot 'Core.cs') (Join-Path $PSScriptRoot 'Program.cs') (Join-Path $PSScriptRoot 'LauncherUpdate.cs') (Join-Path $PSScriptRoot 'tests/WindowState.cs')
if ($LASTEXITCODE -ne 0) { throw 'UI test compilation failed' }
& $testExe $testRoot (Join-Path $workspace 'state/launcher-staging/0.2.0/system/psetup.dll')
if ($LASTEXITCODE -ne 0) { throw 'Launcher UI tests failed' }
