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

- [ ] **Create Release Branch**: Create a new release branch named `release/<version-name-with-patch-placeholder>` (e.g., `release/3.0`) from the `main` branch. This branch will serve as the foundation for upcoming patch releases and facilitate versioned updates.
- [ ] From the release branch (e.g., `release/3.0`), create a new temporary branch for the version (e.g., `release-prep/3.0.0`).
- [ ] Update version references in the documentation and code. Refer to [this PR](https://github.com/OrchardCMS/OrchardCore/pull/17065/files) for an example. The easiest way is to start with a search & replace on the previous version. Version Updates Checklist:
  - **Update `OrchardCore.Commons.props`**: Set `<VersionPrefix></VersionPrefix>` to the new version you're preparing for release.
  - **Update Module Versions**: Modify `src/OrchardCore/OrchardCore.Abstractions/Modules/Manifest/ManifestConstants.cs` to reflect the new version.
  - **Release Notes**: Finalize the release notes in the documentation, including:
      - Highlights and goals of the release.
      - Prerequisites for running the new version.
      - Upgrade steps and any breaking changes.
  - **Update Documentation Navigation**: Add the release notes page to `mkdocs.yml` navigation and remove it from `not_in_nav`.
  - **Version Mentions**: Update all references to the new version throughout the documentation, including:
    - [Status in the root README](https://docs.orchardcore.net/en/latest/#status)
    - CLI templates and commands.
    - Relevant guides, such as the [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/) guide.
- [ ] **Check CI Workflow**: Verify that the [release_ci](https://github.com/OrchardCMS/OrchardCore/blob/main/.github/workflows/release_ci.yml) workflow is using the correct .NET version for the release, and change it in the version branch if necessary.
- [ ] Create a **version PR** titled `Release <version number` (e.g., `Release 3.0.0) from the version branch (e.g., `release/3.0.0`) into the release branch (e.g., `release/3.0`)
- [ ] In GitHub, manually run the `Preview - CI` workflow on your branch (NOT `main`). This will release a new preview version on Cloudsmith for testing.

## Step 3: Translation Updates

**Update Translations**: After finalizing code changes, update translations in the [Translations project](https://github.com/OrchardCMS/OrchardCore.Translations):

- [ ] Update .po files using [PoExtractor](https://github.com/lukaskabrt/PoExtractor) to refresh translations on [Crowdin](https://crowdin.com/project/orchard-core).
- [ ] Publish the new version on NuGet.
- [ ] Update the `OrchardCore.Translations.All` package reference in the main repository's `./Dependencies.Packages.props` file.

## Step 4: Validation

- [ ] **Check Functionality**: Update [`OrchardCore.Samples`](https://github.com/OrchardCMS/OrchardCore.Samples) to the latest preview version generated in the previous step (just change the `OrchardCoreVersion` property in the root `Directory.Build.props` file). Ensure the samples work as expected.
- [ ] **Test Guides**: Test the following guides with NuGet packages from the Cloudsmith feed:
  - [Creating a modular ASP.NET Core application](https://docs.orchardcore.net/en/latest/guides/create-modular-application-mvc/)
  - [Creating an Orchard Core CMS website](https://docs.orchardcore.net/en/latest/guides/create-cms-application/)
  - [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/)
- [ ] **Re-certify for Red Hat**: If a new major version of Red Hat Enterprise Linux is released (e.g., v10 after v9), re-certify Orchard Core. Refer to:
  - Orchard's [Red Hat Ecosystem Catalog profile](https://catalog.redhat.com/software/applications/detail/223797) for the last certified version.
  - [Red Hat Customer Portal](https://access.redhat.com/articles/3078) for the latest version details.
  - [Certification guide](https://docs.orchardcore.net/en/latest/topics/red-hat-ecosystem-catalog-certification/) for certification steps.
- [ ] If everything looks good and all checks pass, merge the version PR.

## Step 5: Creating the New Release

1. Navigate to the [GitHub Releases page](https://github.com/OrchardCMS/OrchardCore/releases/new).
2. In the "**Choose a tag**" menu, enter the new version number (e.g., `v3.0.0`) and select "**+ Create tag: v... on publish**."
3. Change the target branch from `main` to your release branch (e.g., `release/3.0`).
4. Enter the version number in the Title field (e.g., `3.0.0`).
5. Click **Generate release notes** to automatically create release notes based  on the changes.
6. Add a link to the release notes on the docs site: `Check out the full release notes [here](https://docs.orchardcore.net/en/latest/releases/3.0.0/).`
7. Ensure the "Set as the latest release" checkbox is checked, then click **Publish release**.

## Step 6: Aligning Branches

**Merge to `main`**: After releasing the new version, merge the release branch into the main branch to ensure that `main` contains all administrative changes.

- [ ] Create a pull request from the release branch (e.g., `release/3.0`) into `main`.
- [ ] Resolve any merge conflicts using external tools (e.g., Fork) to avoid auto-merging `main` into the release branch. **Important**: DO NOT resolve conflicts using GitHub's interface; that will automatically merge `main` into the release branch, which must be avoided.
- [ ] Merge the PR if all checks pass. If there are merge conflicts, then you'll need to merge to `main` manually using the following steps:
  1. Fetch the latest changes from the Git repository.
  2. `git checkout` the `main` branch.
  3. Merge the release branch (e.g., `release/3.0`) into `main` with a merge commit (NOT a squash merge). Use the commit message pattern `Merge release/3.0.0 to main`.
  4. Resolve any conflicts.
  5. Push the changes to `main`. This action requires a user with the ability to force-push into `main`, as it is protected by default.
  6. GitHub will automatically delete the release branch; go back to the new merged PR to restore it.

## Step 7: Post-Release Tasks

- [ ] **Create New Milestone**: Set up a new milestone for the next release and close the previous one.
- [ ] **Prepare Documentation for Next Version**: Create a new release notes file for the next version in the `OrchardCore.Docs` project (e.g., `/releases/4.0.0.md`). Exclude it from navigation and validation under `not_in_nav` in `mkdocs.yml`.
- [ ] **Update `OrchardCore.Commons.props` for Next Release**: Set `<VersionPrefix></VersionPrefix>` to the next planned release number, but at least a minor one.
- [ ] **Reassign Issues**: Reassign all still open, postponed issues from the current version milestone to the upcoming version milestone.
- [ ] Update [`OrchardCore.Samples`](https://github.com/OrchardCMS/OrchardCore.Samples) to the newly released version (just change the `OrchardCoreVersion` property in the root `Directory.Build.props` file).

## Step 8: Publicizing the Release

- [ ] Post about the release on X (formerly Twitter) via the [Orchard Core X (Twitter) repo](https://github.com/OrchardCMS/Orchard-Core-X-Twitter).
- [ ] Post in the [Orchard Core LinkedIn group](https://www.linkedin.com/groups/13605669/).
- [ ] Post to the [Orchard Core Facebook page](https://www.facebook.com/OrchardCore/).
- [ ] Send a message to the `#announcements` channel on Discord.
