$ErrorActionPreference = 'Stop'
$validator = Join-Path (Split-Path $PSScriptRoot -Parent) 'Test-TranslationsPackage.ps1'
$testDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "orchardcore-translations-tests-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $testDirectory | Out-Null
$propsPath = Join-Path $testDirectory 'Directory.Packages.props'
$global:translationPackageTests = @{
    RestoreCalls = 0
    RestoreExitCode = 0
    ResolvedPackages = @('OrchardCore.Translations.All/3.0.0', 'OrchardCore.Translations.fr/3.0.0')
}

function dotnet {
    $global:translationPackageTests.RestoreCalls++
    $arguments = @($args)
    if ($arguments[0] -ne 'restore' -or
        '--packages' -notin $arguments -or '--no-http-cache' -notin $arguments) {
        throw 'Expected an isolated restore without the HTTP cache.'
    }

    $config = [xml](Get-Content -Raw -Path $arguments[$arguments.IndexOf('--configfile') + 1])
    if (@($config.configuration.packageSources.add).Count -ne 1 -or
        $config.configuration.packageSources.add.value -ne 'https://api.nuget.org/v3/index.json') {
        throw 'Validation must only use NuGet.org.'
    }

    $project = [xml](Get-Content -Raw -Path $arguments[1])
    if ($project.Project.ItemGroup.PackageReference.Version -ne '[3.0.0]') {
        throw 'Expected an exact package version.'
    }

    $objPath = Join-Path (Split-Path $arguments[1] -Parent) 'obj'
    New-Item -ItemType Directory -Path $objPath | Out-Null
    $libraries = @{}
    foreach ($package in $global:translationPackageTests.ResolvedPackages) {
        $libraries[$package] = @{ type = 'package' }
    }
    @{ libraries = $libraries } | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $objPath 'project.assets.json')
    $global:LASTEXITCODE = $global:translationPackageTests.RestoreExitCode
}

function Set-TestReference([string]$Version) {
    @"
<Project>
  <ItemGroup>
    <PackageVersion Include="Unrelated" Version="1.2.3" />
    <PackageVersion Include="OrchardCore.Translations.All" Version="$Version" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path $propsPath
}

function Assert-Failure([scriptblock]$Action, [string]$Message) {
    $before = Get-Content -Raw -Path $propsPath
    $caught = $false
    try {
        & $Action
    } catch {
        if ($_.Exception.Message -notlike "*$Message*") {
            throw
        }
        $caught = $true
    }
    if (-not $caught) {
        throw "Expected failure containing '$Message'."
    }
    if ((Get-Content -Raw -Path $propsPath) -cne $before) {
        throw 'Failed validation must not change the package reference.'
    }
}

try {
    foreach ($invalid in @('[3.0.0-preview-20094]', '3.0.0-*', '[3.0.0,4.0.0)', '[3.0.0', '03.0.0')) {
        Set-TestReference $invalid
        Assert-Failure { & $validator -PackagesPropsPath $propsPath } 'single stable version'
    }
    if ($global:translationPackageTests.RestoreCalls -ne 0) {
        throw 'Invalid release references must fail before restoring.'
    }

    Set-TestReference '[3.0.0-preview-20094]'
    foreach ($invalid in @('', '3.0.0-preview-20094', '3.0.0; echo injected', '03.0.0')) {
        Assert-Failure { & $validator -Version $invalid -UpdateReference -PackagesPropsPath $propsPath } 'major.minor.patch'
    }
    & $validator -Version '3.0.0' -UpdateReference -PackagesPropsPath $propsPath
    $props = [xml](Get-Content -Raw -Path $propsPath)
    if ($props.Project.ItemGroup.PackageVersion[1].Version -ne '[3.0.0]' -or
        $props.Project.ItemGroup.PackageVersion[0].Version -ne '1.2.3') {
        throw 'Only the translation version should change.'
    }

    & $validator -PackagesPropsPath $propsPath
    Set-TestReference '3.0.0'
    & $validator -PackagesPropsPath $propsPath

    Set-TestReference '[3.0.0-preview-20094]'
    $global:translationPackageTests.RestoreExitCode = 1
    Assert-Failure { & $validator -Version '3.0.0' -UpdateReference -PackagesPropsPath $propsPath } 'Could not restore'
    $global:translationPackageTests.RestoreExitCode = 0

    $global:translationPackageTests.ResolvedPackages = @('OrchardCore.Translations.All/3.0.0', 'OrchardCore.Translations.fr/3.0.0-preview-1')
    Assert-Failure { & $validator -Version '3.0.0' -UpdateReference -PackagesPropsPath $propsPath } 'non-stable package'
    $global:translationPackageTests.ResolvedPackages = @('OrchardCore.Translations.All/3.0.0', 'OrchardCore.Translations.fr/3.0.1')
    Assert-Failure { & $validator -Version '3.0.0' -UpdateReference -PackagesPropsPath $propsPath } 'instead of 3.0.0'
    $global:translationPackageTests.ResolvedPackages = @('OrchardCore.Translations.All/3.0.1')
    Assert-Failure { & $validator -Version '3.0.0' -UpdateReference -PackagesPropsPath $propsPath } 'did not resolve'

    (Get-Content -Raw -Path $propsPath).Replace('OrchardCore.Translations.All', 'Other') | Set-Content $propsPath
    Assert-Failure { & $validator -PackagesPropsPath $propsPath } 'exactly one'
    Write-Host 'Translation package tests passed.'
} finally {
    Remove-Item -LiteralPath $testDirectory -Recurse -Force
    Remove-Variable -Name translationPackageTests -Scope Global
}
