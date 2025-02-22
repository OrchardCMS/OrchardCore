---
name: Publish a patch release
about: Publish a new Orchard Core patch release
title: 'Release v'
labels: release
assignees: ''

---

# Release Patch Preparation Guide

## Step 1: Backporting Pull Requests

1. **Identify Pull Requests**: Review any pull requests (PRs) that need to be backported to the release branch. These should be PRs that strictly contain bug fixes and nothing else.
2. **Backport Pull Requests**: For PRs merged into the `main` branch that need to be applied to the release branch (e.g., `release/2.1`), comment on the merged PR with `/backport to release/2.1`. This comment will trigger a GitHub Action to create a new PR with the same changes for the `release/2.1` branch.
3. **Merge PRs**: Once all necessary PRs are created, merge them into the `release/2.1` branch.

## Step 2: Code and Documentation Updates

- [ ] From the release branch (e.g., `release/2.1`), create a new temporary branch for the version (e.g., `release/2.1.1`).
- [ ] Update version references in the documentation and code. Refer to [this PR](https://github.com/OrchardCMS/OrchardCore/pull/17065/files) for an example. The easiest way is to start with a search & replace on the previous version. Version Updates Checklist:
  - **Update `OrchardCore.Commons.props`**: Set `<VersionPrefix></VersionPrefix>` to the new version you're preparing for release.
  - **Update Module Versions**: Modify `src/OrchardCore/OrchardCore.Abstractions/Modules/Manifest/ManifestConstants.cs` to reflect the new version.
  - **Release Notes**: Finalize the release notes in the documentation, including:
      - Highlights and goals of the release.
      - Prerequisites for running the new version.
      - Upgrade steps and any breaking changes.
  - **Update Documentation Navigation**: Add the release notes page to the `mkdocs.yml` navigation and remove it from `not_in_nav`.
  - **Version Mentions**: Update all references to the new version throughout the documentation, including:
    - [Status in the root README](https://docs.orchardcore.net/en/latest/#status)
    - CLI templates and commands.
    - Relevant guides, such as the [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/) guide.
- [ ] Create a **version PR** titled `Release <version number>` (e.g., `Release 2.1.1`) from the version branch (e.g., `release/2.1.1`) into the release branch (e.g., `release/2.1`)
- [ ] In GitHub, manually run the `Preview - CI` workflow on your branch (NOT `main`). This will release a new preview version on Cloudsmith for testing.

## Step 3: Validation

- [ ] **Check Functionality**: Update [`OrchardCore.Samples`](https://github.com/OrchardCMS/OrchardCore.Samples) to the latest preview version generated in the previous step (just change the `OrchardCoreVersion` property in the root `Directory.Build.props` file). Ensure the samples work as expected.
- [ ] **Test Guides**: Test the following guides with NuGet packages from the Cloudsmith feed:
  - [Creating a modular ASP.NET Core application](https://docs.orchardcore.net/en/latest/guides/create-modular-application-mvc/)
  - [Creating an Orchard Core CMS website](https://docs.orchardcore.net/en/latest/guides/create-cms-application/)
  - [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/)
- [ ] If everything looks good and all checks pass, merge the version PR.

## Step 4: Creating the New Release

1. Navigate to the [GitHub Releases page](https://github.com/OrchardCMS/OrchardCore/releases/new).
2. In the "**Choose a tag**" menu, enter the new version number, including `v` (e.g., `v2.1.1`), and select "**+ Create tag: v... on publish**."
3. Change the target branch from `main` to the release branch (e.g., `release/2.1`).
4. Enter the version number in the Title field (e.g., `2.1.1`).
5. Click **Generate release notes** to auto-generate release notes.
6. Add a link to the release notes on the docs site: `Check out the full release notes [here](https://docs.orchardcore.net/en/latest/releases/2.1.1/).`
7. Ensure the "Set as the latest release" checkbox is checked, then click **Publish release**.

## Step 5: Aligning Branches

**Merge to `main`**: After releasing the new version, merge the release branch into the main branch to ensure `main` contains all administrative changes.

- [ ] Create a pull request from the release branch (e.g., `release/2.1`) into `main`.
- [ ] Resolve any merge conflicts using external tools (e.g., Fork) to avoid auto-merging `main` into the release branch. **Important**: DO NOT resolve conflicts using GitHub's interface; that will automatically merge `main` into the release branch, which must be avoided.
- [ ] Merge the PR if all checks pass. If there are merge conflicts, then you'll need to merge to `main` manually using the following steps:
  1. Fetch the latest changes from the Git repository.
  2. Checkout the `main` branch.
  3. Merge the release branch (e.g., `release/2.1`) into `main` with a merge commit (NOT a squash merge). Use the commit message pattern `Merge release/2.1.1 to main`.
  4. Resolve any conflicts.
  5. Push the changes to `main`. This action requires a user with the ability to force-push into `main`, as it is protected by default.
  6. GitHub will automatically delete the release branch; go back to the new merged PR to restore it.

## Step 6: Housekeeping

- [ ] Assign the milestone for the release version to this issue. Create one if it doesn't exist.
- [ ] Assign the milestone to the issues that were released in the version.
- [ ] Close the milestone.
- [ ] Update [`OrchardCore.Samples`](https://github.com/OrchardCMS/OrchardCore.Samples) to the newly released version (just change the `OrchardCoreVersion` property in the root `Directory.Build.props` file).

## Step 7: Publicizing the Release

- [ ] Post about the release on X (formerly Twitter) via the [Orchard Core X (Twitter) repo](https://github.com/OrchardCMS/Orchard-Core-X-Twitter).
- [ ] Post in the [Orchard Core LinkedIn group](https://www.linkedin.com/groups/13605669/).
- [ ] Post to the [Orchard Core Facebook page](https://www.facebook.com/OrchardCore/).
- [ ] Send a message to the `#announcements` channel on Discord.
