param([string]$OutputDirectory, [switch]$TestBench)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'workspace.ps1')
$output = if ($OutputDirectory) { [IO.Path]::GetFullPath($OutputDirectory) } else { Join-Path $workspace 'outputs/launcher' }
New-Item -ItemType Directory -Force $output | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$keybindExe = Join-Path $output 'Keybind Editor.exe'
$keybindSources = (Get-ChildItem (Join-Path $PSScriptRoot 'keybinds') -Filter '*.cs').FullName
& $compiler /nologo /target:winexe /platform:x64 /out:$keybindExe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.Numerics.dll /resource:"$PSScriptRoot\keybinds\LICENSE-L2crypt.txt,L2cryptLicense" @keybindSources
if ($LASTEXITCODE -ne 0) { throw 'Bundled keybind editor compilation failed' }
$defines = if ($TestBench) { '/define:TEST_BENCH' } else { '/define:PUBLIC_LAUNCHER' }
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ /resource:"$keybindExe,KeybindEditor" $defines /out:"$output\Interlude Launcher.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll (Join-Path $PSScriptRoot "Core.cs") (Join-Path $PSScriptRoot "Program.cs") (Join-Path $PSScriptRoot "LauncherUpdate.cs")
if ($LASTEXITCODE -ne 0) { throw 'Launcher compilation failed' }
Write-Output "Built $output/Interlude Launcher.exe"
