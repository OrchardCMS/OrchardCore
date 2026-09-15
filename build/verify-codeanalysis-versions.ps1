<#
.SYNOPSIS
Verifies that Microsoft.CodeAnalysis package versions are supported by the .NET SDK's Roslyn compiler version.

.DESCRIPTION
The Microsoft.CodeAnalysis packages (Analyzers, CSharp, and CSharp.CodeStyle) must be compatible with the 
Roslyn compiler version included in the .NET SDK. This means the packages must be equal to or lower than
the version supported by the SDK. This script checks that:
1. The CodeAnalysis package versions in Directory.Packages.props are not higher than the SDK's Roslyn version
2. The versions are documented and don't drift accidentally

.NOTES
The Roslyn compiler version included in each .NET SDK determines the maximum compatible version of the 
CodeAnalysis packages. You can use older versions of the packages, but not newer ones.

See: https://github.com/dotnet/sdk/releases
#>

param(
    [string]$GlobalJsonPath = "global.json",
    [string]$PackagePropsPath = "Directory.Packages.props"
)

# Define Roslyn/CodeAnalysis version compatibility matrix
# Based on https://github.com/dotnet/sdk and SDK release notes
# The version specified is the MAXIMUM supported version (packages can be equal or lower)
$versionRequirements = @{
    10 = @(
        @{
            # SDK 10.0.3xx (Roslyn 5.6.x) - max analyzer version 5.6
            minPatchVersion = 300
            maxPatchVersion = 399
            maxAnalyzerVersion = "5.6"
            maxAnalyzerMajorMinor = @(5, 6)
            roslynVersion = "5.6"
        },
        @{
            # SDK 10.0.4xx and later (Roslyn 5.9.x and later) - max analyzer version 5.9
            minPatchVersion = 400
            maxPatchVersion = 999
            maxAnalyzerVersion = "5.9"
            maxAnalyzerMajorMinor = @(5, 9)
            roslynVersion = "5.9"
        }
    )
    # Add future SDK major versions here as needed
}

$ErrorActionPreference = "Stop"

# Read global.json to get SDK version
Write-Host "Reading SDK version from $GlobalJsonPath..."
$globalJson = Get-Content $GlobalJsonPath | ConvertFrom-Json
$sdkVersion = $globalJson.sdk.version
Write-Host "SDK version: $sdkVersion"

# Extract major and minor versions
$versionParts = $sdkVersion -split '\.'
$sdkMajorVersion = [int]$versionParts[0]
$sdkMinorVersion = [int]$versionParts[1]
$sdkPatchVersion = if ($versionParts.Count -gt 2) { [int]$versionParts[2] } else { 0 }

Write-Host "SDK major.minor.patch: $sdkMajorVersion.$sdkMinorVersion.$sdkPatchVersion"

# Helper function to compare semantic versions
function Compare-SemanticVersion {
    param(
        [string]$VersionString,
        [int[]]$MaxVersion  # [major, minor]
    )

    $parts = $VersionString -split '\.'
    $major = if ($parts.Count -gt 0) { [int]$parts[0] } else { 0 }
    $minor = if ($parts.Count -gt 1) { [int]$parts[1] } else { 0 }

    # Compare major.minor only (ignore patch)
    if ($major -lt $MaxVersion[0]) {
        return -1  # Version is lower
    }
    elseif ($major -gt $MaxVersion[0]) {
        return 1   # Version is higher
    }
    else {
        # Major versions are equal, compare minor
        if ($minor -lt $MaxVersion[1]) {
            return -1  # Version is lower
        }
        elseif ($minor -gt $MaxVersion[1]) {
            return 1   # Version is higher
        }
        else {
            return 0   # Versions are equal
        }
    }
}

# Check if we have requirements for this SDK version
if (-not $versionRequirements.ContainsKey($sdkMajorVersion)) {
    Write-Error "No version requirements defined for .NET SDK $sdkMajorVersion. Please update this script."
    exit 1
}

# Find the matching requirement based on patch version
$majorVersionRequirements = $versionRequirements[$sdkMajorVersion]
$matchingRequirement = $null

foreach ($req in $majorVersionRequirements) {
    if ($sdkPatchVersion -ge $req.minPatchVersion -and $sdkPatchVersion -le $req.maxPatchVersion) {
        $matchingRequirement = $req
        break
    }
}

if ($null -eq $matchingRequirement) {
    Write-Error "No version mapping found for SDK $sdkMajorVersion.$sdkMinorVersion.$sdkPatchVersion. Please update this script."
    exit 1
}

$maxAnalyzerVersion = $matchingRequirement.maxAnalyzerVersion
$maxAnalyzerMajorMinor = $matchingRequirement.maxAnalyzerMajorMinor
$roslynVersion = $matchingRequirement.roslynVersion

Write-Host "Maximum supported CodeAnalysis analyzer version for SDK ${sdkMajorVersion}.${sdkMinorVersion}.${sdkPatchVersion}: $maxAnalyzerVersion"
Write-Host "SDK includes Roslyn version: $roslynVersion"
Write-Host ""

# Read Directory.Packages.props to extract CodeAnalysis package versions
Write-Host "Reading package versions from $PackagePropsPath..."
$packageProps = Get-Content $PackagePropsPath -Raw

# Extract versions using regex
$packages = @(
    @{ Name = "Microsoft.CodeAnalysis.Analyzers" },
    @{ Name = "Microsoft.CodeAnalysis.CSharp" },
    @{ Name = "Microsoft.CodeAnalysis.CSharp.CodeStyle" }
)

$allValid = $true

foreach ($pkg in $packages) {
    # Match lines like: <PackageVersion Include="Microsoft.CodeAnalysis.Analyzers" Version="5.6.0" />
    $escapedName = [regex]::Escape($pkg.Name)
    if ($packageProps -match "Include=""$escapedName""\s+Version=""([^""]+)""") {
        $version = $matches[1]
        Write-Host "Found $($pkg.Name): $version"

        # Compare version with maximum supported
        $comparison = Compare-SemanticVersion $version $maxAnalyzerMajorMinor

        if ($comparison -le 0) {
            # Version is equal or lower than max supported
            Write-Host "  ✓ Version is supported by SDK $sdkMajorVersion.$sdkMinorVersion.${sdkPatchVersion} (max: $maxAnalyzerVersion, Roslyn $roslynVersion)" -ForegroundColor Green
        } else {
            # Version is higher than max supported
            Write-Host "  ✗ Version is NOT supported! Version $version exceeds maximum $maxAnalyzerVersion for SDK $sdkMajorVersion.$sdkMinorVersion.${sdkPatchVersion}" -ForegroundColor Red
            Write-Host "    Current SDK includes Roslyn $roslynVersion which only supports up to $maxAnalyzerVersion" -ForegroundColor Red
            $allValid = $false
        }
    } else {
        Write-Host "  ✗ Package $($pkg.Name) not found in $PackagePropsPath!" -ForegroundColor Red
        $allValid = $false
    }
}

if (-not $allValid) {
    Write-Host "`nVersion mismatch detected! Please ensure CodeAnalysis packages don't exceed the maximum version supported by SDK $sdkMajorVersion.$sdkMinorVersion.${sdkPatchVersion} (max: $maxAnalyzerVersion)." -ForegroundColor Red
    exit 1
}

Write-Host "`n✓ All CodeAnalysis package versions are supported by the .NET SDK (Roslyn $roslynVersion, max version: $maxAnalyzerVersion)" -ForegroundColor Green
exit 0
