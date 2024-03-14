# Publishing a new Orchard Core release

These notes are primarily for Orchard's core contributors to guide how to prepare a new release.

## Versioning

We follow [Semantic Versioning 2.0.0](https://semver.org/).

## Release checklist

You can copy the following checklist to a GitHub issue and tick everything as you progress.  
While the checklist is in a recommended order not every step depends strictly on the previous ones.  
`<version name>` should be replaced with the current version, e.g. "1.0.0" or "rc2".

```markdown
### Prepare the project

Do some housekeeping on GitHub in the [main repo](https://github.com/OrchardCMS/OrchardCore).

- [ ] Close remaining issues for the version (including merging corresponding pull requests if suitable) or assign them to the next one.
- [ ] Assign all issues that were closed for an upcoming version (including a wildcard version like "1.0.x") to this version (milestone).

### Prepare the code

Update the source so everything looks like on the new version.

- [ ] Create a `release/<version name>` branch out of `main`, e.g. `release/1.8`.
- [ ] Check the [release_ci](https://github.com/OrchardCMS/OrchardCore/blob/main/.github/workflows/release_ci.yml) workflow is using the expected .NET version to build the Docker images.
- [ ] Update the `OrchardCore.Commons.props` file with `<VersionSuffix></VersionSuffix>` such that preview build numbers are not injected in packages. Verify the `VersionPrefix` tag matches the released version.
- [ ] Update module versions in `src/OrchardCore/OrchardCore.Abstractions/Modules/Manifest/ManifestConstants.cs`.
- [ ] Update the version in the command lines in from all documentation files.
- [ ] Create a new milestone.

### Test the release

Make sure everything works all right.

- [ ] Make sure that [`OrchardCore.Samples` works](https://github.com/OrchardCMS/OrchardCore.Samples).
- [ ] Test the [guides](https://docs.orchardcore.net/en/latest/docs/guides/) with the NuGet packages from the Cloudsmith feed (branches under `release/` are automatically published too). Test at least the following guides:
    - [Creating a modular ASP.NET Core application](https://docs.orchardcore.net/en/latest/docs/guides/create-modular-application-mvc/)
    - [Creating an Orchard Core CMS website](https://docs.orchardcore.net/en/latest/docs/guides/create-cms-application/)
    - [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/docs/guides/decoupled-cms/)
- [ ] If there's a more recent major version of Red Hat Enterprise Linux (like v10 after v9) that Orchard Core was certified for, then re-certify it. See Orchard's [Red Hat Ecosystem Catalog profile](https://catalog.redhat.com/software/applications/detail/223797) for the version it was certified for, the [Red hat Customer Portal](https://access.redhat.com/articles/3078) for the latest released version, and [https://docs.orchardcore.net/en/latest/docs/topics/red-hat-ecosystem-catalog-certification/](our certification guide) for the steps to renew the certification.

### Prepare and publish Orchard Core Translations

Update everything in the [Translations project](https://github.com/OrchardCMS/OrchardCore.Translations). Only do this once all the code changes are done since localized strings can change until then.

- [ ] Update .po files with [PoExtractor](https://github.com/lukaskabrt/PoExtractor). This will also update [Crowdin](https://crowdin.com/project/orchard-core).
- [ ] Publish the new version on NuGet.
- [ ] Update the `OrchardCore.Translations.All` package reference in the main repo's _src/OrchardCore.Build/Dependencies.props_ file to refer to the new NuGet package.

### Prepare the documentation

Update the docs so they contain information about the new release so once the release is out you'll just need to point to new information.

- [ ] Create a new __Draft__ Release with a tag `vx.y.z` that is created when the release is published. Auto-generate release notes.
- [ ] Create release notes in a specific documentation section. You can take the previous release notes as a template.
    - Overview of the release's highlights and goals. What do you want people to remember this release for?
    - Prerequisites. What framework version do you need, anything else to work with Orchard?
    - Upgrade steps, any migration necessary from previous versions, breaking changes.

### Publish the release

Do the harder parts of making the release public. This should come after everything above is done.

- [ ] Merge `release/<version name>` to `main`.
    - Merges to `main` need two approvals so you'll need to create a pull request.
    - Merge it as a merge commit, not squash merge.
- [ ] Publish the Draft release.
- [ ] Test the [guides](https://docs.orchardcore.net/en/latest/docs/guides/) with the packages now automatically published to NuGet. Test at least the following guides:
    - [Creating a modular ASP.NET Core application](https://docs.orchardcore.net/en/latest/docs/guides/create-modular-application-mvc/)
    - [Creating an Orchard Core CMS website](https://docs.orchardcore.net/en/latest/docs/guides/create-cms-application/)
    - [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/docs/guides/decoupled-cms/)
- [ ] Update [Try Orchard Core](https://github.com/OrchardCMS/TryOrchardCore).

### Publicize the release

Let the whole world know about our shiny new release. Savor this part! These steps will make the release public so only do them once everything else is ready.

- [ ] Update the documentation to mention the version in all places where the latest version is referenced, for example, but not limited to (do a search for the package version string): [Status in the root README](https://docs.orchardcore.net/en/latest/#status), CLI templates, commands, the [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/docs/guides/decoupled-cms/) guide.
- [ ] Update the tagged [release](https://github.com/OrchardCMS/OrchardCore/releases) on GitHub: Change its title to something more descriptive (e.g. "Orchard Core 1.0.0 RC 2"), add a link in its description to the release notes in the documentation (something like `For details on this version see the [release notes in the documentation](link here).`). Add a link to this release under [Status in the root README](https://docs.orchardcore.net/en/latest/#status).
- [ ] Publish a blog post on the website.
- [ ] Ask to publish a blog post on [DevBlogs](https://devblogs.microsoft.com/).
- [ ] Ask to publish a blog post on [.NET Foundation News](https://dotnetfoundation.org/news).
- [ ] Tweet

### After the release is done

- [ ] Create a new milestone with the next release number.
- [ ] Create a new release notes documentation file for the next version in the OrchardCore.Docs project. (e.g, `/releases/1.8.0.md`).
- [ ] Update the `OrchardCore.Commons.props` file with the next release number, and `<VersionSuffix>preview</VersionSuffix>` such that preview builds use the new one.

```
