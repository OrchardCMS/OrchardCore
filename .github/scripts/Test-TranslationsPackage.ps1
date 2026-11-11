[CmdletBinding()]
param(
    [string]$Version,
    [switch]$UpdateReference,
    [string]$PackagesPropsPath = (Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) 'Directory.Packages.props')
)

$ErrorActionPreference = 'Stop'

$document = [System.Xml.XmlDocument]::new()
$document.PreserveWhitespace = $true
$document.Load($PackagesPropsPath)
$references = @($document.SelectNodes('/Project/ItemGroup/PackageVersion[@Include="OrchardCore.Translations.All"]'))
if ($references.Count -ne 1) {
    throw 'Expected exactly one OrchardCore.Translations.All reference in Directory.Packages.props.'
}

$reference = $references[0]
$stableVersionPattern = '(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)'
if (-not $PSBoundParameters.ContainsKey('Version')) {
    if ($UpdateReference) {
        $buildDirectory = Join-Path (Join-Path (Split-Path $PackagesPropsPath -Parent) 'src') 'OrchardCore.Build'
        $commonsPath = Join-Path $buildDirectory 'OrchardCore.Commons.props'
        $commons = [xml](Get-Content -Raw -Path $commonsPath)
        $prefixes = @($commons.SelectNodes('/*[local-name()="Project"]/*[local-name()="PropertyGroup"]/*[local-name()="VersionPrefix"]'))
        if ($prefixes.Count -ne 1) {
            throw 'Expected exactly one VersionPrefix in OrchardCore.Commons.props.'
        }
        $Version = $prefixes[0].InnerText
    } else {
        $currentVersion = $reference.GetAttribute('Version')
        if ($currentVersion -cmatch "\A\[(?<version>$stableVersionPattern)\]\z" -or
            $currentVersion -cmatch "\A(?<version>$stableVersionPattern)\z") {
            $Version = $Matches.version
        } else {
            throw "Release translations must reference a single stable version, not '$currentVersion'. Run Prepare Release Translations before tagging."
        }
    }
}

if ($Version -cnotmatch "\A$stableVersionPattern\z") {
    throw "Translation version '$Version' must be a stable version in major.minor.patch format."
}

$temporaryDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "orchardcore-translations-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $temporaryDirectory | Out-Null
try {
    $projectPath = Join-Path $temporaryDirectory 'Translations.csproj'
    $configPath = Join-Path $temporaryDirectory 'NuGet.config'
    $packagesPath = Join-Path $temporaryDirectory 'packages'
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <NuGetAudit>false</NuGetAudit>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="OrchardCore.Translations.All" Version="[$Version]" />
  </ItemGroup>
</Project>
"@ | Set-Content -Path $projectPath
    @'
<configuration>
  <packageSources>
    <clear />
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
'@ | Set-Content -Path $configPath

    # An isolated cache and config prevent Cloudsmith or previously cached packages from satisfying the check.
    dotnet restore $projectPath --configfile $configPath --packages $packagesPath --no-http-cache --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Could not restore translation package $Version and its dependencies from NuGet.org. Publish that version first or explicitly select an existing compatible stable version."
    }

    $assetsPath = Join-Path (Join-Path $temporaryDirectory 'obj') 'project.assets.json'
    $assets = Get-Content -Raw -Path $assetsPath | ConvertFrom-Json
    $packages = @($assets.libraries.PSObject.Properties | Where-Object { $_.Value.type -eq 'package' })
    if ("OrchardCore.Translations.All/$Version" -notin $packages.Name) {
        throw "Restore did not resolve OrchardCore.Translations.All/$Version."
    }

    foreach ($package in $packages) {
        $id, $resolvedVersion = $package.Name -split '/', 2
        if ($resolvedVersion -cnotmatch "\A$stableVersionPattern\z") {
            throw "Release translations depend on a non-stable package: $($package.Name)."
        }

        if ($id.StartsWith('OrchardCore.Translations.', [System.StringComparison]::OrdinalIgnoreCase) -and $resolvedVersion -ne $Version) {
            throw "Translation package $id resolved to $resolvedVersion instead of $Version."
        }
    }

    Write-Host "Validated OrchardCore.Translations.All/$Version and $($packages.Count - 1) dependencies from NuGet.org."
    if ($UpdateReference) {
        $reference.SetAttribute('Version', "[$Version]")
        $settings = [System.Xml.XmlWriterSettings]::new()
        $settings.Encoding = [System.Text.UTF8Encoding]::new($false)
        $settings.OmitXmlDeclaration = $true
        $settings.NewLineHandling = [System.Xml.NewLineHandling]::None
        $writer = [System.Xml.XmlWriter]::Create($PackagesPropsPath, $settings)
        try {
            $document.Save($writer)
        } finally {
            $writer.Dispose()
        }
    }
} finally {
    Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force
}
