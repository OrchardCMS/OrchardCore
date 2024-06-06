# Opening and managing issues

The [issue tracker on GitHub](https://github.com/OrchardCMS/OrchardCore/issues) is where we keep track of bug reports, feature requests, or items for other tasks. We ask you to choose a task from there when you [contribute](README.md), but opening an issue is a contribution too, so let's see some tips on that!

## How to open an issue

If you find a bug in Orchard Core, or have an idea for a new or improved feature, please open an issue [on GitHub](https://github.com/OrchardCMS/OrchardCore/issues/new/choose).

- Please select the appropriate type, bug or feature request. This helps us assess your issue better!
- Fill out the template, and please add as many details as possible. If you don't do this, then both you and another contributor will have to take time discussing what your issue is about.

## What happens after you open an issue

We triage issues every week on [the triage meeting](../../resources/meeting/README.md), as well as core contributors may comment on your issue before that. Please reply to any inquiries.

Once your issue is triaged, one of the following things will happen:

- A [milestone](https://github.com/OrchardCMS/OrchardCore/milestones) is assigned to it, see below. This means we think your issue is definitely worth addressing, thank you!
- We close it. If your issue is a duplicate of an earlier one, is a report about a bug that's already fixed, or something that we don't think is a good match for Orchard Core, we'll close it with an explanation. This is not against you, it's just that we can't address all issues. Please don't hesitate to open new issues for other matters.

## Issue milestones

This is what [issue milestones](https://github.com/OrchardCMS/OrchardCore/milestones) mean:

- The next patch version (e.g. if the current version is `1.2.3`, then `1.2.4`) indicates the highest priority for serious regressions and other urgent bug fixes that we intend to fix ASAP and publish in a patch release.
- The next minor version (e.g. `1.3` if the current version is `1.2.0`) is for less urgent bug fixes and feature requests that we still think should be addressed in the next planned release. Regressions since the last release found by those from the community who live on the edge and use the [preview releases](../../getting-started/preview-package-source.md) are marked as such too.
- Some later minor version (literally `1.x` if the current version is `1.anything`) is for issues that we intend to address eventually, maybe.
- The `backlog` milestone is for everything else that we think is a valid request, but we won't work on it any time soon.

## Managing issues as a core contributor

Some tips on issue management:

- An issue should be about a concrete task, some change in Orchard Core or how we run the project. If it's a question or discussion, then [convert it into a discussion](https://docs.github.com/en/discussions/managing-discussions-for-your-community/moderating-discussions#converting-an-issue-to-a-discussion).
- If you asked the author something and the issue should be closed if they don't reply, add the `needs author feedback` label. This will automatically mark the issue as stale after 15 days, and then close it after another 7.
- You can list all issues to be triaged [here](https://github.com/OrchardCMS/OrchardCore/issues?q=is%3Aopen+is%3Aissue+no%3Amilestone+-label%3A%22needs+author+feedback%22+-label%3A%22community+metrics%22+sort%3Acreated-asc).
- Set the milestone according to the above logic, or close the issue with a comment elaborating the reason.
- Add further labels for categorization (external contributors can't add labels). E.g.:
    - Add "good first issue" if the issue looks suitable for a novice contributor.
    - Add "perf" if it's about performance.
    - Add module/feature set-related labels, like "Media" or "OpenId".
    - Add "security" for security issues.
- Change the issue's title if it contains errors or is unclear/incorrect.
