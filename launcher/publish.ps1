param(
 [string]$Repository,
 [Parameter(Mandatory=$true)][string]$Version,
 [switch]$Publish
)
$ErrorActionPreference = 'Stop'
$arguments = @((Join-Path $PSScriptRoot 'publish.py'), '--version', $Version)
if ($Repository) { $arguments += @('--repo', $Repository) }
if ($Publish) { $arguments += '--publish' }
& python @arguments
if ($LASTEXITCODE -ne 0) { throw 'Release publication failed; inspect the reported draft before retrying.' }
