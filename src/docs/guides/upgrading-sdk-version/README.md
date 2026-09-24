# Upgrading the .NET SDK Version

This guide explains how to upgrade the .NET SDK version used by Orchard Core and the related changes needed, particularly regarding Roslyn and CodeAnalysis packages.

## Overview

Orchard Core specifies a minimum .NET SDK version in `global.json`. When upgrading the SDK version, you need to:

1. Update the SDK version in `global.json`
2. Update `.github/actions/setup-dotnet/action.yml` to the same SDK version
3. Update CodeAnalysis packages to versions compatible with the new SDK's Roslyn compiler (optional)
4. Update other framework-specific packages as needed
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

### Step 3: Update CodeAnalysis Packages (Optional)

Updating CodeAnalysis packages is **optional**. You may choose to update them to newer compatible versions when upgrading the SDK, but it is not required.

If you decide to update them, use `Directory.Packages.props` to update the CodeAnalysis ItemGroup with the maximum compatible versions:

```xml
<!-- Microsoft.CodeAnalysis packages must be equal to or lower than the version supported by the Roslyn 
     compiler in global.json (10.0.4xx). Version 5.9 is the max supported version for SDK 10.0.4xx+. -->
<ItemGroup>
  <PackageVersion Include="Microsoft.CodeAnalysis.Analyzers" Version="5.9.0" />
  <PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="5.9.0" />
  <GlobalPackageReference Include="Microsoft.CodeAnalysis.CSharp.CodeStyle" Version="5.9.0" />
</ItemGroup>
```

### Step 4: Update GitHub setup-dotnet Action

Update `.github/actions/setup-dotnet/action.yml` so `dotnet-version` matches `global.json` exactly.

Example:

```yaml
with:
  dotnet-version: |
    10.0.302
```

This ensures CI installs the same SDK version used by the repository.

### Step 5: Update Framework-Specific Packages

When upgrading to a new SDK major version, you may need to update other packages:

- Update `src/OrchardCore.Build/TargetFrameworks.props` if adding new target frameworks
- Add new `AspNetCorePackagesVersion` entries in `Directory.Packages.props` if dual-targeting
- Update package versions that have SDK-specific dependencies
- See [target frameworks checklist](https://github.com/OrchardCMS/OrchardCore/blob/main/.github/ISSUE_TEMPLATE/target_frameworks.md) for full details

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
- Install the SDK version from `.github/actions/setup-dotnet/action.yml`
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

### Build fails after updating CodeAnalysis packages

CodeAnalysis packages may be newer than the Roslyn version included in the SDK pinned by `global.json`. Lower the `Microsoft.CodeAnalysis.*` package versions in `Directory.Packages.props` to versions supported by that SDK.

### CI fails to install the expected SDK version

Ensure `.github/actions/setup-dotnet/action.yml` was updated to the exact same SDK version as `global.json`.

### Local build succeeds but CI fails

Make sure you have the correct .NET SDK version installed locally and that CI uses the same version. Keep `global.json` and `.github/actions/setup-dotnet/action.yml` aligned, then install the required SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download).

## References

- [global.json specification](https://learn.microsoft.com/dotnet/core/tools/global-json)
- [.NET SDK releases](https://github.com/dotnet/sdk/releases)
- [Roslyn](https://github.com/dotnet/roslyn)
- [Target frameworks upgrade checklist](https://github.com/OrchardCMS/OrchardCore/blob/main/.github/ISSUE_TEMPLATE/target_frameworks.md)
