---
name: Publish a release
about: Publish a new Orchard Core release
title: 'Release v'
labels: release
assignees: ''

---
<!-- Be sure to also read https://docs.orchardcore.net/en/latest/topics/publishing-releases/. While the checklist is in a recommended order not every step depends strictly on the previous ones.  
`<version name>` should be replaced with the current version, e.g. "1.8.0.". -->

### Prepare the project

Do some housekeeping on GitHub in the [main repo](https://github.com/OrchardCMS/OrchardCore).

- [ ] Assign the milestone of the release's version to this issue.
- [ ] Close remaining issues for the version (including merging corresponding pull requests if suitable) or assign them to the next one.
- [ ] Assign all issues that were closed for an upcoming version (including a wildcard version like "1.0.x") to this version (milestone).

### Prepare the code and documentation

Update the source, so everything looks like on the new version.

- [ ] Create a `release/<version name>` branch (e.g., `release/1.8.0`) from `main` or the previous release branch. This is necessary to target pull requests for the upcoming release.
- [ ] Create an issue branch out of this branch as usual.
- [ ] Check the [release_ci](https://github.com/OrchardCMS/OrchardCore/blob/main/.github/workflows/release_ci.yml) workflow is using the expected .NET version to build the Docker images.
- [ ] Update the `OrchardCore.Commons.props` file with `<VersionSuffix></VersionSuffix>` such that preview build numbers are not injected in packages. Verify the `VersionPrefix` tag matches the released version.
- [ ] Update module versions in `src/OrchardCore/OrchardCore.Abstractions/Modules/Manifest/ManifestConstants.cs`.
- [ ] Create a new milestone.
- [ ] Add final updates to the release notes in the documentation. It should include the following, at least:
  - Overview of the release's highlights and goals. What do you want people to remember this release for?
  - Prerequisites. What framework version do you need, anything else to work with Orchard?
  - Upgrade steps, any migration necessary from previous versions, and any breaking changes.
- [ ] Add the release notes documentation page to the documentation site's navigation in `mkdocs.yml` and remove it from `not_in_nav`.
- [ ] Update the documentation to mention the version in all places where the latest version is referenced, for example, but not limited to (do a search for the package version string): [Status in the root README](https://docs.orchardcore.net/en/latest/#status), CLI templates, commands, the [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/) guide.

### Test the release

Make sure everything works all right.

- [ ] Make sure that [`OrchardCore.Samples` works](https://github.com/OrchardCMS/OrchardCore.Samples).
- [ ] Test the [guides](https://docs.orchardcore.net/en/latest/guides/) with the NuGet packages from the Cloudsmith feed (branches under `release/` are automatically published too). Test at least the following guides:
  - [Creating a modular ASP.NET Core application](https://docs.orchardcore.net/en/latest/guides/create-modular-application-mvc/)
  - [Creating an Orchard Core CMS website](https://docs.orchardcore.net/en/latest/guides/create-cms-application/)
  - [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/)
- [ ] Re-certify Orchard Core for the latest major version of Red Hat Enterprise Linux if a new version has been released (e.g., v10 after v9). Refer to Orchard's [Red Hat Ecosystem Catalog profile](https://catalog.redhat.com/software/applications/detail/223797) for the previously certified version, the [Red Hat Customer Portal](https://access.redhat.com/articles/3078) for the latest version, and [our certification guide](https://docs.orchardcore.net/en/latest/topics/red-hat-ecosystem-catalog-certification/) for the certification steps.

### Prepare and publish Orchard Core Translations

Update everything in the [Translations project](https://github.com/OrchardCMS/OrchardCore.Translations). Only do this once all the code changes are done since localized strings can change until then.

- [ ] Update .po files with [PoExtractor](https://github.com/lukaskabrt/PoExtractor). This will also update [Crowdin](https://crowdin.com/project/orchard-core).
- [ ] Publish the new version on NuGet.
- [ ] Update the `OrchardCore.Translations.All` package reference in the main repo's _./Dependencies.Packages.props_ file to refer to the new NuGet package.

### Publish the release

Do the harder parts of making the release public. This should come after everything above is done.

- [ ] Tag the head of `release/<version name>` with the version. Include `v` in the name, e.g. `v1.8.0`.
- [ ] Merge `release/<version name>` to `main`.
  - You'll need to create a pull request.
  - Merge it as a merge commit, not squash merge.
  - If there are merge conflicts, then create a `release/<version name>-integration` branch and fix them there.
- [ ] Create and publish a release [on GitHub](https://github.com/OrchardCMS/OrchardCore/releases/new).
  - Generate release notes.
  - Add a link to the release notes in the docs (something like `For details on this version see the [release notes in the documentation](link here).`). Note that the docs will only be built once the branch is merged to `main`.
- [ ] Test the [guides](https://docs.orchardcore.net/en/latest/guides/) with the packages now automatically published to NuGet. Test at least the following guides:
  - [Creating a modular ASP.NET Core application](https://docs.orchardcore.net/en/latest/guides/create-modular-application-mvc/)
  - [Creating an Orchard Core CMS website](https://docs.orchardcore.net/en/latest/guides/create-cms-application/)
  - [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/)
- [ ] Update [Try Orchard Core](https://github.com/OrchardCMS/TryOrchardCore).

### Publicize the release

Let the whole world know about our shiny new release. Savor this part! These steps will make the release public so only do them once everything else is ready.

- [ ] Tweet
- [ ] Post to the [Orchard Core LinkedIn group](https://www.linkedin.com/groups/13605669/)
- [ ] Post to the [Orchard Core Facebook page](https://www.facebook.com/OrchardCore/)

### After the release is done

- [ ] Create a new milestone with the next release number and close the old milestone.
- [ ] Create a new release notes documentation file for the next version in the `OrchardCore.Docs` project. (e.g., `/releases/1.8.0.md`). Don't add it to the docs navigation and exclude it from validation under `not_in_nav` with `mkdocs.yml`.
- [ ] Update the `OrchardCore.Commons.props` file with the next release number, and `<VersionSuffix>preview</VersionSuffix>` such that preview builds use the new one.
