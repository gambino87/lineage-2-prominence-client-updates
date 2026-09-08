$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'workspace.ps1')
$testRoot = Join-Path $workspace ('state/launcher-tests/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $testRoot | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$testExe = Join-Path $testRoot 'Integration.exe'
& $compiler /nologo /target:exe /out:$testExe /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll (Join-Path $PSScriptRoot 'Core.cs') (Join-Path $PSScriptRoot 'tests/Integration.cs')
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed' }
& $testExe $testRoot (Join-Path $workspace 'client/system/psetup.dll')
if ($LASTEXITCODE -ne 0) { throw 'Launcher integration tests failed' }
