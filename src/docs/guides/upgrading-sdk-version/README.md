# Upgrading the .NET SDK Version

This guide explains how to upgrade the .NET SDK version used by Orchard Core and the related changes needed, particularly regarding Roslyn and CodeAnalysis packages.

## Overview

Orchard Core specifies a minimum .NET SDK version in `global.json`. When upgrading the SDK version, you need to:

1. Update the SDK version in `global.json`
2. Update CodeAnalysis packages to versions compatible with the new SDK's Roslyn compiler
3. Update other framework-specific packages as needed
4. Run the verification script to ensure compatibility
5. Update CI/CD configuration and documentation

## Understanding the Relationship

The .NET SDK includes a specific version of the Roslyn compiler. The CodeAnalysis packages (analyzers and code style checkers) must be **equal to or lower than** the version of Roslyn included in the SDK. This is critical for compatibility.

**Example:**
- .NET SDK 10.0.0-10.0.2xx includes Roslyn versions lower than 5.6 → CodeAnalysis packages must match
- .NET SDK 10.0.3xx+ includes Roslyn 5.6+ → CodeAnalysis packages must be ≤ Roslyn version
- .NET SDK 10.0.4xx+ includes Roslyn 5.9 → CodeAnalysis packages must be ≤ 5.9

## Step-by-Step Upgrade Process

### Step 1: Update global.json

Update the SDK version in `global.json`:

```json
{
  "comment": "We only update the version manually with major SDK updates to keep using any more recent SDK version possible.",
  "sdk": {
    "version": "10.0.4xx",
    "rollForward": "latestMajor"
  },
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

**Note:** Use `rollForward: latestMajor` to allow patch updates within the same major version, giving developers flexibility to use newer patches.

### Step 2: Determine Compatible CodeAnalysis Versions

Look up the Roslyn version included in the new SDK:
- Check [.NET SDK releases](https://github.com/dotnet/sdk/releases) for the Roslyn version included in the SDK version you are upgrading to
- Check [Roslyn](https://github.com/dotnet/roslyn) for additional Roslyn version details

Update the version requirement mapping in `build/verify-codeanalysis-versions.ps1` if this is a new minor SDK version.

### Step 3: Update CodeAnalysis Packages (Optional)

Updating CodeAnalysis packages is **optional**. You may choose to update them to newer compatible versions when upgrading the SDK, but it is not required.

If you decide to update them, use `Directory.Packages.props` to update the CodeAnalysis ItemGroup with the maximum compatible versions:

```xml
<!-- Microsoft.CodeAnalysis packages must be equal to or lower than the version supported by the Roslyn 
     compiler in global.json (10.0.4xx). Version 5.9 is the max supported version for SDK 10.0.4xx+.
     See build/verify-codeanalysis-versions.ps1 for version verification. -->
<ItemGroup>
  <PackageVersion Include="Microsoft.CodeAnalysis.Analyzers" Version="5.9.0" />
  <PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="5.9.0" />
  <GlobalPackageReference Include="Microsoft.CodeAnalysis.CSharp.CodeStyle" Version="5.9.0" />
</ItemGroup>
```

### Step 4: Update the Verification Script (If Needed)

If the new SDK version is a new minor version (e.g., upgrading from 10.0.3xx to 10.0.4xx), you must manually add a new entry to the `$versionRequirements` hash in `build/verify-codeanalysis-versions.ps1` with the compatible CodeAnalysis version range for that SDK version.

Before committing, verify that the SDK version and CodeAnalysis packages are properly configured:

```powershell
pwsh ./build/verify-codeanalysis-versions.ps1
```

This script will:
- Parse the SDK version from `global.json`
- Check the version mapping in the script
- Verify that CodeAnalysis packages don't exceed the maximum supported version
- Provide clear diagnostics if versions don't match

**Expected output:**
```
✓ All CodeAnalysis package versions are supported by the .NET SDK (Roslyn 5.9, max version: 5.9)
```

### Step 5: Update Framework-Specific Packages

When upgrading to a new SDK major version, you may need to update other packages:

- Update `src/OrchardCore.Build/TargetFrameworks.props` if adding new target frameworks
- Add new `AspNetCorePackagesVersion` entries in `Directory.Packages.props` if dual-targeting
- Update package versions that have SDK-specific dependencies
- See [target frameworks checklist](.github/ISSUE_TEMPLATE/target_frameworks.md) for full details

### Step 6: Update CI/CD and Documentation

- Update Docker base images in `Dockerfile`s
- Update `.github/workflows` to test with the new SDK version
- Update any documentation that mentions specific .NET versions
- Add a release note about the new SDK version requirement

### Step 7: Verify the Build

Run the full build to ensure everything compiles:

```powershell
dotnet build -c Release
```

The CI pipeline will also automatically:
- Run `build/verify-codeanalysis-versions.ps1` to check version compatibility
- Run all unit and functional tests
- Verify there are no compilation errors

## Renovate Configuration

Automatic updates of CodeAnalysis packages are **disabled** in `renovate.json5` to prevent accidental version mismatches. These packages should only be updated manually during SDK upgrades.

Current configuration in `renovate.json5`:

```json5
{
    // Microsoft.CodeAnalysis packages must stay in sync with the Roslyn compiler in global.json,
    // which requires equal or lower versions. These are updated manually during .NET SDK upgrades only.
    matchPackageNames: [
        'Microsoft.CodeAnalysis.Analyzers',
        'Microsoft.CodeAnalysis.CSharp',
        'Microsoft.CodeAnalysis.CSharp.CodeStyle',
    ],
    enabled: false,
}
```

## Troubleshooting

### Build fails with "CodeAnalysis package version mismatch"

The CI verification script detected that CodeAnalysis packages exceed the maximum version supported by the SDK in `global.json`. Run the verification script locally to see the exact mismatch:

```powershell
pwsh ./build/verify-codeanalysis-versions.ps1
```

Then update the CodeAnalysis versions in `Directory.Packages.props` to equal or lower versions.

### "No version mapping found for SDK X.Y.Z"

The verification script doesn't have a mapping for your SDK version. Add a new entry to the `$versionRequirements` hash in `build/verify-codeanalysis-versions.ps1`:

```powershell
@{
    # SDK 10.0.5xx and later (Roslyn X.Y.x)
    minPatchVersion = 500
    maxPatchVersion = 999
    maxAnalyzerVersion = "X.Y"
    maxAnalyzerMajorMinor = @(X, Y)
    roslynVersion = "X.Y"
}
```

### Local build succeeds but CI fails

Make sure you have the correct .NET SDK version installed. The `global.json` file enforces a minimum SDK version. Install the required version from [dotnet.microsoft.com](https://dotnet.microsoft.com/download).

## References

- [global.json specification](https://learn.microsoft.com/dotnet/core/tools/global-json)
- [.NET SDK releases](https://github.com/dotnet/sdk/releases)
- [Roslyn](https://github.com/dotnet/roslyn)
- [Target frameworks upgrade checklist](.github/ISSUE_TEMPLATE/target_frameworks.md)
- [CodeAnalysis package version verification script](build/verify-codeanalysis-versions.ps1)
