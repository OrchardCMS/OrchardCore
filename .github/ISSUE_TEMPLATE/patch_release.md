---
name: Publish a patch release
about: Publish a new Orchard Core patch release
title: 'Release v'
labels: release
assignees: ''

---

# Release Patch Preparation Guide

## Step 1: Backporting Pull Requests

1. **Identify PRs**: Review any pull requests (PRs) that need to be backported to the release branch.
2. **Backporting Process**: For PRs merged into the main branch that need to be applied to `release/2.1`, comment on the merged PR with `/backport to release/2.1`. This action will trigger GitHub to create a new PR with the same changes for the `release/2.1` branch.
3. **Merge PRs**: Once all necessary PRs are created, merge them into the `release/2.1` branch.

## Step 2: Administration Tasks

#### Create Pull Request:

   - From the release branch (e.g., `release/2.1`), create a new branch for your release (e.g., `release-notes/2.1.1`).
   - Update version references in the documentation. Refer to [this PR](https://github.com/OrchardCMS/OrchardCore/pull/17065/files) for an example.
   - Version Updates Checklist:
     - [ ] **Update `OrchardCore.Commons.props`**: Set `<VersionSuffix></VersionSuffix>` to prevent preview build numbers. Ensure `VersionPrefix` matches the released version.
     - [ ] **Update Module Versions**: Modify `src/OrchardCore/OrchardCore.Abstractions/Modules/Manifest/ManifestConstants.cs` as needed.
     - [ ] **Release Notes**: Finalize release notes in the documentation, including:
       - Highlights and goals of the release.
       - Prerequisites for running the new version.
       - Upgrade steps and any breaking changes.
     - [ ] **Update Documentation Navigation**: Add the release notes page to `mkdocs.yml` navigation and remove it from `not_in_nav`.
     - [ ] **Version Mentions**: Update all references to the new version throughout the documentation, including:
       - [Status in the root README](https://docs.orchardcore.net/en/latest/#status)
       - CLI templates and commands.
       - Relevant guides, such as the [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/) guide.

## Step 3: Create New Release

1. Navigate to the [GitHub Releases page](https://github.com/OrchardCMS/OrchardCore/releases/new).
2. In the "**Choose a tag**" menu, enter the new version number including `v` (e.g., `v2.1.1`) and select "**+ Create tag: v... on publish**."
3. Change the target branch from `main` to your target branch (e.g., `release/2.1`).
4. Enter the version number in the Title field (e.g., `2.1.1`).
5. Click **Generate release notes** to auto-generate release notes.
6. Ensure the "Set as the latest release" checkbox is checked, then click **Publish release**.

## Step 4: Align Branches

1. **Merge to Main**: After releasing the new version, merge the release branch into the main branch to ensure main contains all administration changes.
   - Create a pull request from the release branch into main.
   - **Important**: DO NOT resolve conflicts using GitHub's interface; use other tools (e.g., Fork) to manage conflicts and avoid auto-merging main into the release branch. Resolving conflicts using GitHub will automatically merge main into the release branch which MUST be avoided.
   - Once conflicts are resolved, merge the PR into main.

## Step 5: Housekeeping

- [ ] Assign the milestone of the release version to the issue.
- [ ] Close any remaining issues for this version or assign them to the next release.

## Step 6: Validation

1. **Check Functionality**: Ensure that the [`OrchardCore.Samples`](https://github.com/OrchardCMS/OrchardCore.Samples) works as expected.
2. **Test Guides**: Test the following guides with NuGet packages from the Cloudsmith feed:
   - [Creating a modular ASP.NET Core application](https://docs.orchardcore.net/en/latest/guides/create-modular-application-mvc/)
   - [Creating an Orchard Core CMS website](https://docs.orchardcore.net/en/latest/guides/create-cms-application/)
   - [Creating a new decoupled CMS Website](https://docs.orchardcore.net/en/latest/guides/decoupled-cms/)

## Step 7: Publicize the Release

- [ ] Post about the release to X via [the Orchard Core X (Twitter) repo](https://github.com/OrchardCMS/Orchard-Core-X-Twitter).
- [ ] Post in the [Orchard Core LinkedIn group](https://www.linkedin.com/groups/13605669/).
- [ ] Share on the [Orchard Core Facebook page](https://www.facebook.com/OrchardCore/).
