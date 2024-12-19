---
name: Publish a major or minor release
about: Publish a new Orchard Core release
title: 'Release v'
labels: release
assignees: ''

---

# Release Major or Minor Preparation Guide

## Step 1: Housekeeping on GitHub

- [ ] **Assign Milestone**: Assign the release version milestone to this issue.
- [ ] **Address Outstanding Issues**: Resolve any remaining issues for the current version.
- [ ] **Merge Relevant Pull Requests**: Merge all applicable pull requests or assign any outstanding issues to the next release cycle.

## Step 2: Code and Documentation Updates

- [ ] **Create Release Branch**: Create a new release branch named `release/<version-name-with-out-patch-number>` (e.g., `release/3.0`) from the `main` branch. This branch will serve as the foundation for upcoming patch releases and facilitate versioned updates.
- [ ] **Check CI Workflow**: Verify that the [release_ci](https://github.com/OrchardCMS/OrchardCore/blob/main/.github/workflows/release_ci.yml) workflow is using the correct .NET version for the release.
- [ ] **Update Documentation**: Open a new pull request for the following updates:
  - **Update `OrchardCore.Commons.props`**: Set `<VersionSuffix></VersionSuffix>` to the new version you're preparing for release.
  - **Update Module Versions**: Modify `src/OrchardCore/OrchardCore.Abstractions/Modules/Manifest/ManifestConstants.cs` to reflect the new version.
  - **Release Notes**: Finalize release notes in the documentation, including:
    - Key highlights and goals of the release.
    - Prerequisites for running the new version.
    - Upgrade steps and any breaking changes.
  - **Update Documentation Navigation**: Add the release notes page to `mkdocs.yml` navigation and remove it from `not_in_nav`.
  - **Version Mentions**: Update all references to the new version throughout the documentation, including:
    - [Status in the root README](https://docs.orchardcore.net/en/latest/#status).
    - CLI templates and commands.
    - Relevant guides, such as the [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/) guide.

## Step 3: Translation Updates

- [ ] **Update Translations**: After finalizing code changes, update translations in the [Translations project](https://github.com/OrchardCMS/OrchardCore.Translations):
  - Update .po files using [PoExtractor](https://github.com/lukaskabrt/PoExtractor) to refresh translations on [Crowdin](https://crowdin.com/project/orchard-core).
  - Publish the new version on NuGet.
  - Update the `OrchardCore.Translations.All` package reference in the main repository's `./Dependencies.Packages.props` file.

## Step 4: Validation

- [ ] **Test Samples**: Ensure that [`OrchardCore.Samples`](https://github.com/OrchardCMS/OrchardCore.Samples) functions correctly by using the preview packages generated for the release.
- [ ] **Validate Guides**: Test the following guides using the latest NuGet packages from the Cloudsmith feed:
  - [Creating a modular ASP.NET Core application](https://docs.orchardcore.net/en/latest/guides/create-modular-application-mvc/)
  - [Creating an Orchard Core CMS website](https://docs.orchardcore.net/en/latest/guides/create-cms-application/)
  - [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/)
- [ ] **Re-certify for Red Hat**: If a new major version of Red Hat Enterprise Linux is released (e.g., v10 after v9), re-certify Orchard Core. Refer to:
  - Orchard's [Red Hat Ecosystem Catalog profile](https://catalog.redhat.com/software/applications/detail/223797) for the last certified version.
  - [Red Hat Customer Portal](https://access.redhat.com/articles/3078) for the latest version details.
  - [Certification guide](https://docs.orchardcore.net/en/latest/topics/red-hat-ecosystem-catalog-certification/) for certification steps.

## Step 5: Creating the New Release

1. Navigate to the [GitHub Releases page](https://github.com/OrchardCMS/OrchardCore/releases/new).
2. In the "**Choose a tag**" menu, enter the new version number (e.g., `v3.0.0`) and select "**+ Create tag: v... on publish**."
3. Change the target branch from `main` to your release branch (e.g., `release/3.0`).
4. Enter the version number in the Title field (e.g., `3.0.0`).
5. Click **Generate release notes** to automatically create release notes based on the changes.
6. Ensure the "Set as the latest release" checkbox is checked, then click **Publish release**.

## Step 6: Post-Release Tasks

- [ ] **Create New Milestone**: Set up a new milestone for the next release and close the previous one.
- [ ] **Prepare Documentation for Next Version**: Create a new release notes file for the next version in the `OrchardCore.Docs` project (e.g., `/releases/4.0.0.md`). Exclude it from navigation and validation under `not_in_nav` in `mkdocs.yml`.
- [ ] **Update Commons.props for Next Release**: Modify `OrchardCore.Commons.props` with the next release number and set `<VersionSuffix>preview</VersionSuffix>` for preview builds.
- [ ] **Reassign Issues**: Reassign all closed issues from the current version milestone to the upcoming version milestone.

## Step 7: Publicize the Release

- [ ] Post about the release on X (formerly Twitter) via the Orchard Core X (Twitter) repo.
- [ ] Post in the [Orchard Core LinkedIn group](https://www.linkedin.com/groups/13605669/).
- [ ] Share on the [Orchard Core Facebook page](https://www.facebook.com/OrchardCore/).
