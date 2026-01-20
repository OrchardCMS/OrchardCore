# Reviewing pull requests

Giving feedback on pull requests, i.e. reviewing them, is a very important contribution too. While it's mostly core contributors doing reviews, don't shy away from a review if you aren't one! Anybody can review pull requests, only core contributors can merge them.

!!! tip
    Reviews also show up as green blobs under your GitHub profile, did you know that? :)

## Guidelines for reviewing code

For general guidelines on reviewing code, see the [Dojo Library](https://orcharddojo.net/orchard-resources/CoreLibrary/DevelopmentGuidelines/CodeReview). Stack Overflow also has [some good principles](https://stackoverflow.blog/2019/09/30/how-to-make-good-code-reviews-better/).

## Accessing the code of a pull request locally

If the PR is coming from a fork, working with its code locally won't be as trivial as doing a `git checkout` on the branch of a core contributor. See [GitHub's docs on this](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/checking-out-pull-requests-locally).

## Tips on managing pull requests

!!! note
    Adding labels and otherwise updating a pull request, as well as merging them are only available for core contributors. Otherwise, anybody can review the changes in pull requests and comment on them.

- [Here's a list](https://github.com/OrchardCMS/OrchardCore/pulls?q=is%3Apr+is%3Aopen+reviewed-by%3A%40me) of open PRs reviewed by you. Maybe check them because perhaps it's time to ping the authors since maybe they forgot to follow up. Or maybe you forgot to merge a PR?
- Change the PR's title if it contains errors or is unclear/incorrect.
- Add the "don't merge" label on PRs that are ready for review, approved by you and/or others, but you asked more people for feedback before it can be merged.
- Add the "needs triage" label on PRs that you think should be checked by others during the weekly triage meeting too. Explain in a comment why you deem this is necessary.
- Be sure to merge PRs that don't need a second opinion. This is especially important for external contributors who can't merge their PRs. Keeping PRs open will make them collect merge conflicts and make the contributor demotivated.
- Be especially patient and encouraging with first-time contributors. This is indicated by such a flag on the PR.
- You can add the [Feedback section](contributing-code.md) to your review comment (i.e. the one that you send your per-line comments with) to remind the author about the practices.
- You can ask [CodeRabbit](https://coderabbit.ai/) for an AI code review with a `@coderabbitai review` comment under any PR.

## Backporting Pull Requests to Another Branch

Occasionally, you may find it necessary to cherry-pick a pull request into a release branch after it has been merged into `main`. To facilitate this process, we've established a dedicated workflow called `backport`.

To initiate the backporting process, simply leave a comment on the pull request you wish to backport. The comment should contain the following command: `/backport to release/1.8`. This command serves as a trigger for the backporting workflow, seamlessly bringing the desired pull request into the specified branch.
