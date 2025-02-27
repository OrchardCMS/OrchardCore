$CurrentPath = Get-Location
$SourcePath = Join-Path (get-item $CurrentPath).parent.parent "\src"
$TargetPath = Join-Path (get-item $CurrentPath).parent.parent "\tmp\translations"
$HostPath = Join-Path $SourcePath "\OrchardCore.Cms.Web\Localization"

if (Test-Path -Path $TargetPath) {
    Remove-Item (get-item $TargetPath).parent –recurse
    New-Item -Path $TargetPath -ItemType Directory -Force
} else {
    New-Item -Path $TargetPath -ItemType Directory -Force
}

npm install
dotnet tool uninstall --global OrchardCoreContrib.PoExtractor
dotnet tool install --global OrchardCoreContrib.PoExtractor

extractpo $SourcePath $TargetPath

$env:GOOGLE_APPLICATION_CREDENTIALS=".\google-credentials.json"
$LocalizationPath = Join-Path $HostPath "\ja"
node po-gtranslator.js --project_id=oc-translation --po_source="$TargetPath" --po_dest="$LocalizationPath" --lang=ja
