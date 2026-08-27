$ErrorActionPreference = 'Stop'

$audit = dotnet list 'Tips Player/Tips Player.csproj' package --vulnerable --include-transitive 2>&1 | Out-String
if ($LASTEXITCODE -ne 0) { throw "NuGet vulnerability audit failed to execute.`n$audit" }
if ($audit -notmatch 'has no vulnerable packages') { throw "NuGet reported a vulnerable package.`n$audit" }

[xml]$manifest = Get-Content -LiteralPath 'Tips Player/Platforms/Android/AndroidManifest.xml'
$android = 'http://schemas.android.com/apk/res/android'
$application = $manifest.manifest.application
if ($application.GetAttribute('allowBackup', $android) -ne 'false') { throw 'Android backups must remain disabled.' }
if ($application.GetAttribute('usesCleartextTraffic', $android) -ne 'false') { throw 'Android cleartext traffic must remain disabled.' }

foreach ($component in @($application.service) + @($application.receiver)) {
    if ($component.GetAttribute('exported', $android) -ne 'false') {
        throw "Android component $($component.GetAttribute('name', $android)) must not be exported."
    }
}
