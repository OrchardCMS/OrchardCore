---
name: Change target frameworks
about: Update supported TFMs
title: 'Change target frameworks'
labels: 
assignees: ''
---

- [ ] Update `global.json` to the required SDK version.
Use the minimal SDK version required, the `rollForward` rule will pick the latest version available on the machine. It's up to the user to update their SDK to the version they want in case there is an update and we don't want to force them into a newer version in case they can't install it.
- [ ] Update `src/OrchardCore.Build/TargetFrameworks.props`.
- [ ] Add a custom `AspNetCorePackagesVersion` for each TFM in `Directory.Packages.props`
- [ ] Update all `uses: actions/setup-dotnet` tasks to the required SDK version.
- [ ] Update all `dotnet publish`, `dotnet build` and `dotnet test` calls to the latest TFM, if specified.
- [ ] Update all `tasks.json` files to target the latest TFM
- [ ] Update the list of template **choices** (see the `template.json` files).
- [ ] Update docker file base images.
- [ ] Update documentation pages specifying a TFM (search for `<TargetFramework>`).
- [ ] Add a note about the supported .NET versions to the upcoming release notes.
- [ ] When dual-targeting framework versions, configure Renovate in the `renovate.json5` file (in the repository root) to update packages targeting the older framework only up to their compatible versions. An example package rule for the `packageRules` section that will update the <9.0.0 (i.e. 8.x) version of the matching packages only up to 8.x:
  ```json5
  {
      // The .NET 8 versions of these packages need to stay on 8.x. We maintain a separate reference to the 
      // .NET 9 versions in Directory.Packages.props, which is only active when building for .NET 9.
      matchPackageNames: [
          '/^Microsoft\\.AspNetCore.*$/',
          '/^Microsoft\\.Extensions.*$/',
          'Serilog.AspNetCore',
          'System.Text.Json',
      ],
      allowedVersions: '<9.0.0',
      matchCurrentVersion: '<9.0.0',
  }
  ```
