param(
    [Parameter(Mandatory)][string]$TargetFile,
    [Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{40}$')][string]$Thumbprint
)

$ErrorActionPreference = 'Stop'
$store = [System.Security.Cryptography.X509Certificates.X509Store]::new(
    [System.Security.Cryptography.X509Certificates.StoreName]::My,
    [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser)
$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadOnly)
try {
    $certificate = $store.Certificates.Find(
        [System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint,
        $Thumbprint,
        $false) | Select-Object -First 1
}
finally {
    $store.Close()
}
if ($null -eq $certificate) { throw 'The configured signing certificate was not found.' }
if (-not $certificate.HasPrivateKey) { throw 'The configured signing certificate has no private key.' }

$resolvedTarget = (Resolve-Path -LiteralPath $TargetFile).Path
$resolvedOutput = Split-Path -Parent $resolvedTarget
$signTool = Get-ChildItem -LiteralPath (Join-Path $env:USERPROFILE '.nuget\packages\microsoft.windows.sdk.buildtools') `
    -Filter 'signtool.exe' -File -Recurse -ErrorAction Stop |
    Where-Object FullName -Match '\\x64\\signtool\.exe$' |
    Sort-Object FullName -Descending |
    Select-Object -First 1
if ($null -eq $signTool) { throw 'signtool.exe was not found in the restored Windows SDK build tools.' }

$files = Get-ChildItem -LiteralPath $resolvedOutput -File -Recurse |
    Where-Object Extension -in '.exe', '.dll'

foreach ($file in $files) {
    & $signTool.FullName verify /pa /q $file.FullName *> $null
    if ($LASTEXITCODE -eq 0) { continue }

    & $signTool.FullName sign /sha1 $Thumbprint /s My /fd SHA256 /q $file.FullName *> $null
    if ($LASTEXITCODE -ne 0) { throw "Failed to sign $($file.FullName)." }
}
