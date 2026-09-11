param([string]$OutputDirectory, [switch]$TestBench)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'workspace.ps1')
$output = if ($OutputDirectory) { [IO.Path]::GetFullPath($OutputDirectory) } else { Join-Path $workspace 'outputs/launcher' }
New-Item -ItemType Directory -Force $output | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$defines = if ($TestBench) { '/define:TEST_BENCH' } else { '/define:PUBLIC_LAUNCHER' }
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ $defines /out:"$output\Interlude Launcher.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll (Join-Path $PSScriptRoot "Core.cs") (Join-Path $PSScriptRoot "Program.cs") (Join-Path $PSScriptRoot "LauncherUpdate.cs")
if ($LASTEXITCODE -ne 0) { throw 'Launcher compilation failed' }
Write-Output "Built $output/Interlude Launcher.exe"
