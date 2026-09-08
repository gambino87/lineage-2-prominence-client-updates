param(
 [Parameter(Mandatory=$true)][string]$Manifest,
 [Parameter(Mandatory=$true)][string]$PublicKeyFile
)
$ErrorActionPreference = 'Stop'
$rsa = New-Object System.Security.Cryptography.RSACryptoServiceProvider
try {
 $rsa.PersistKeyInCsp = $false
 $rsa.FromXmlString([IO.File]::ReadAllText($PublicKeyFile))
 $signature = [Convert]::FromBase64String([IO.File]::ReadAllText($Manifest + '.sig').Trim())
 if (!$rsa.VerifyData([IO.File]::ReadAllBytes($Manifest), [System.Security.Cryptography.CryptoConfig]::MapNameToOID('SHA256'), $signature)) {
  throw 'Release signature verification failed.'
 }
} finally { $rsa.Dispose() }
