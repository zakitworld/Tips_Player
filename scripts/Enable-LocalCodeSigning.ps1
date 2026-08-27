param([string]$Subject = 'CN=Tips Player Local Development')

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$configurationPath = Join-Path $repositoryRoot 'Directory.Build.user.props'

$certificate = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject $Subject `
    -CertStoreLocation 'Cert:\CurrentUser\My' `
    -KeyAlgorithm RSA `
    -KeyLength 3072 `
    -HashAlgorithm SHA256 `
    -KeyExportPolicy NonExportable `
    -NotAfter (Get-Date).AddYears(2)

$publicCertificatePath = Join-Path ([System.IO.Path]::GetTempPath()) "tips-player-$($certificate.Thumbprint).cer"
try {
    Export-Certificate -Cert $certificate -FilePath $publicCertificatePath -Force | Out-Null
    Import-Certificate -FilePath $publicCertificatePath -CertStoreLocation 'Cert:\CurrentUser\Root' | Out-Null
    Import-Certificate -FilePath $publicCertificatePath -CertStoreLocation 'Cert:\CurrentUser\TrustedPublisher' | Out-Null
}
finally {
    Remove-Item -LiteralPath $publicCertificatePath -Force -ErrorAction SilentlyContinue
}

$configuration = @"
<Project>
  <PropertyGroup>
    <LocalCodeSigningThumbprint>$($certificate.Thumbprint)</LocalCodeSigningThumbprint>
  </PropertyGroup>
</Project>
"@
[System.IO.File]::WriteAllText($configurationPath, $configuration)

Write-Host "Local signing enabled with certificate $($certificate.Thumbprint)."
Write-Host 'Rebuild the Windows project before running it.'
