# Publishing a new Orchard Core release

These notes are primarily for Orchard's core contributors to guide how to prepare a new release.

## Versioning

We follow [Semantic Versioning 2.0.0](https://semver.org/).

## Release checklist

You can copy the following checklist to a GitHub issue and tick everything as you progress. While the checklist is in a recommended order not every step depends strictly on the previous ones. "<version name>" should be replaced with the current version, e.g. "1.0.0" or "rc2".

```
### Prepare the project
Do some housekeeping on GitHub in the [main repo](https://github.com/OrchardCMS/OrchardCore).

- [ ] Close remaining issues for the version (including merging corresponding pull requests if suitable) or assign then to the next one.
- [ ] Assign all issues that were closed for an upcoming version (including a wildcard version like "1.0.x") to this version (milestone).

### Prepare the code
Update the source so everything looks like on the new version.

- [ ] Create code generation templates.
- [ ] Make sure that [OrchardCore.Samples works](https://github.com/OrchardCMS/OrchardCore.Samples).
- [ ] Create a `release/<version name>` branch out of `dev`.
- [ ] Update the `OrchardCore.Commons.props` file: Update `VersionPrefix` for release versions (like "1.0.0") and `VersionSuffix` for pre-release versions (like "rc2", for the full version to be e.g. "1.0.0-rc2").
- [ ] Update module versions in `ManifestConstants`.
- [ ] Change docker version in _.travis.yml_ and _appveyor.yml_.

### Test the release
Make sure everything works all right.

- [ ] Test the [guides](https://docs.orchardcore.net/en/dev/docs/guides/).
- [ ] Test NuGet packages.

### Prepare and publish Orchard Core Translations

Update everything in the [Translations project](https://github.com/OrchardCMS/OrchardCore.Translations). Only do this once all the code changes are done since localized strings can change until then.

- [ ] Update `packageVersion` and/or `versionPrefix` in _appveyor.yml_.
- [ ] Update .po files with [PoExtractor](https://github.com/lukaskabrt/PoExtractor). This will also update [Crowdin](https://crowdin.com/project/orchard-core).
- [ ] Publish the new version on NuGet.
- [ ] Update the `OrchardCore.Translations.All` package reference in the main repo's _src/OrchardCore.Build/Dependencies.props_ file to refer to the new NuGet package.

### Prepare the documentation
Update the docs so they contain information about the new release so once the release is out you'll just need to point to new information.

- [ ] Create release notes in a specific documentation section. You can take the previous release notes as a template.
  - Overview on the release's highlights and goals.
  - Upgrade steps
  - New features
  - Full changelog. You can generate this with [github-changelog](https://github.com/cfpb/github-changelog) with the `changelog OrchardCMS OrchardCore <previous version> <current version>` command, e.g. `changelog OrchardCMS OrchardCore 1.0.0-rc1 1.0.0-rc2`.

### Publish the release
Do the harder parts of making the release public. This should come after everything above is done.

- [ ] Merge `release/<version name>` to `master`.
- [ ] Tag `master` with the full version name, including the prefix and suffix (e.g. "1.0.0-rc2").
- [ ] Publish the new version on NuGet.
- [ ] Update [Try Orchard Core](https://github.com/OrchardCMS/TryOrchardCore).

### Publicize the release
Let the whole world know about our shiny new release Savor this part! These steps will make the release public so only do them once everything else is ready.

- [ ] Update the documentation to mention the version in all places where the latest version is referenced, for example, but not limited to: [Status in the root README](https://docs.orchardcore.net/en/dev/#status), CLI templates, and commands.
- [ ] Update the [release](https://github.com/OrchardCMS/OrchardCore/releases) on GitHub: Change its title to something more descriptive (e.g. "Orchard Core 1.0.0 RC 2"), add a link to it to the release notes in the documentation. Add a link to this release under [Status in the root README](https://docs.orchardcore.net/en/dev/#status).
- [ ] Tweet
- [ ] Update the website.
```