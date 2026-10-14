param([string]$ClientSystem)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'workspace.ps1')
if (!$ClientSystem) {
    $candidatePath = Join-Path $workspace 'outputs/test-bench/candidate.json'
    if (Test-Path -LiteralPath $candidatePath) {
        $candidate = Get-Content -LiteralPath $candidatePath -Raw | ConvertFrom-Json
        $candidateStage = if ([System.IO.Path]::IsPathRooted($candidate.staging)) { $candidate.staging } else { Join-Path $workspace $candidate.staging }
        $ClientSystem = Join-Path $candidateStage 'system'
    }
}
$testRoot = Join-Path $workspace ('state/keybind-update-builds/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $testRoot | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$sources = (Get-ChildItem (Join-Path $PSScriptRoot 'keybinds') -Filter '*.cs').FullName
$testExe = Join-Path $testRoot 'Tests.exe'
& $compiler /nologo /target:exe /main:KeybindUpdateTests /out:$testExe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.Numerics.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll (Join-Path $PSScriptRoot 'Core.cs') (Join-Path $PSScriptRoot 'tests/KeybindUpdateTests.cs') @sources
if ($LASTEXITCODE -ne 0) { throw 'Keybind update test compilation failed' }
if ($ClientSystem) { & $testExe $workspace $ClientSystem } else { & $testExe $workspace }
if ($LASTEXITCODE -ne 0) { throw 'Keybind update tests failed' }
