# Contributing code to Orchard Core

First of all, thank you for thinking about contributing to the code! Be sure to check out [the general contribution docs](README.md) first.

!!! info
    Are you looking for information on contributing documentation? Head over to [here](contributing-documentation.md) instead.

## Working with Orchard Core's source code

First, clone the repository using the command `git clone https://github.com/OrchardCMS/OrchardCore.git` and checkout the `main` branch. Then, you have multiple options, see below. You can also use [any other .NET IDE](../../resources/development-tools/README.md) too.

### Command Line

1. Install the latest version of the .NET SDK from <https://dotnet.microsoft.com/download>.
2. Navigate to `./OrchardCore/src/OrchardCore.Cms.Web`.
3. Run `dotnet run`.
4. Open the `http://localhost:5000` URL in your browser.

### Visual Studio

1. Download Visual Studio 2022 (v17.8+) from <https://www.visualstudio.com/downloads> (any edition will work).
2. Launch the solution by clicking on `OrchardCore.sln`. Give Visual Studio time to restore all missing NuGet packages.
3. Ensure `OrchardCore.Cms.Web` is set as the startup project. Then run the app with Ctrl+F5.

## Selecting what to work on

We warmly welcome fixes and features! Here are some resources to help you get started on how to contribute code:

- Check out [the issue tracker](https://github.com/OrchardCMS/OrchardCore/issues) for open bug reports and feature requests. Prefer to select issues [scheduled for the upcoming release](https://github.com/OrchardCMS/OrchardCore/milestones) (see the [issue management docs](managing-issues.md#issue-milestones) on what the milestones mean). You can [sort issues by most commented](https://github.com/OrchardCMS/OrchardCore/issues?q=is%3Aissue+is%3Aopen+sort%3Acomments-desc) and [most thumbs up](https://github.com/OrchardCMS/OrchardCore/issues?q=is%3Aissue+is%3Aopen+sort%3Areactions-%2B1-desc) (as well as other reactions similarly). These correlate with popularity, i.e. we can see what the community most wants.
- ["Good first issue" issues](https://github.com/OrchardCMS/OrchardCore/labels/good%20first%20issue): We think these are a good for newcomers.
- ["Help wanted" issues](https://github.com/OrchardCMS/OrchardCore/labels/help%20wanted): These issues are up for grabs. Comment on an issue if you want to create a fix.
- [Documentation issues](https://github.com/OrchardCMS/OrchardCore/labels/documentation) are quite suitable for newcomers too. See the [docs on contributing documentation](contributing-documentation.md).

## Identifying the scale

First, identify the scale of what you would like to contribute. If it is small (grammar/spelling or a bug fix) feel free to start working on a fix. If you are submitting a feature or other substantial code contribution, please discuss it with the team first and ensure it follows the roadmap and fits into the bigger picture. We'd hate to see your work go to waste! The best is if you work on open issues, and open an issue for your idea too, before starting coding.

You might also read these two blog posts on contributing code: [Open Source Contribution Etiquette](http://tirania.org/blog/archive/2010/Dec-31.html) by Miguel de Icaza and [Don't "Push" Your Pull Requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/) by Ilya Grigorik.

## Submitting a pull request (PR)

!!! info
    If you don't know what a pull request is, read [this article](https://help.github.com/articles/using-pull-requests).

- Familiarize yourself with the project and our coding conventions, as you can see in the repository. We follow the [ASP.NET Core Engineering guidelines](https://github.com/dotnet/aspnetcore/wiki/Engineering-guidelines).
- Make sure the repository can be built and all tests pass. This is also checked by the CI workflows.
- If you change CSS or JavaScript files, be sure to run [the Gulp pipeline](../../guides/gulp-pipeline/README.md).
- If you want to start a conversation with other community members or run the CI workflows but you're not done yet, open your PR as a [draft](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests#draft-pull-requests). Then, [change it to ready for review](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/changing-the-stage-of-a-pull-request).
- If your PR addresses an issue, be sure to [link them](https://docs.github.com/en/issues/tracking-your-work-with-issues/linking-a-pull-request-to-an-issue) (e.g. with `Fixes #IssueId`). This helps everyone find their way around contributions, and merging your PR will automatically close the issue too.
- If there's no issue for your PR, then please describe the context and rationale in the pull request description, and provide screenshots/screen recordings of the changes if they affect the UX.
- Refactoring is great, but if you do so, please guard it with new tests.
- If you add a significant new feature or a breaking change, then document this under the release notes of the upcoming release (you can find this in the `docs/OrchardCore.Docs/releases` folder).
- So we can help you better, please [allow our core contributors to edit your PR](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/allowing-changes-to-a-pull-request-branch-created-from-a-fork).

!!! tip
    When in doubt, just ask the community for guidance, we're happy to help!

## Feedback

All code submissions will be reviewed and tested by the core team, and only those that meet our bar for both quality and design/roadmap appropriateness will be merged. Please be patient; we very much appreciate you contributing to Orchard Core!

!!! note
    Following the below recommendations will ensure that your PR is merged as quickly as possible. Please work with us so it's a pleasant experience for everyone.

- Please update your pull request according to feedback until it is approved by one of the core team members.
- Apply [suggested changes](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/incorporating-feedback-in-your-pull-request#applying-suggested-changes) directly so the reviewer doesn't have to eyeball the changes. These resolve themselves after applying them, and that's fine.
- Don't resolve other conversations so it's easier to track for the reviewer. Then, the reviewer will resolve them.
- Feel free to mark conversations that you addressed to keep track of them with an emoji or otherwise, just don't resolve them.
- Please keep conversations happening in line comments in those convos, otherwise, communication will be a mess. If you have trouble finding them, see [this video](https://github.com/OrchardCMS/OrchardCore/pull/14749#issuecomment-1917976028).
- When you're done addressing all feedback of a review, click ["Re-request review"](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/incorporating-feedback-in-your-pull-request#re-requesting-a-review) in the top-right corner for each reviewer when you're ready for another round of review, so they know that you're done.
- [CodeRabbit](https://coderabbit.ai/) is available for AI on-demand code reviews, which they graciously provide for us as an open-source project for free. You can ask the bot for a code review with a `@coderabbitai review` comment under any pull request. You can have a conversation with it under its comments too. Note that AI code reviews can help, but are frequently incorrect; use your best judgment.

!!! tip
    Do you want to demo what you've done to others, to showcase your work and to gather feedback? Join one of [our meetings](../../resources/meeting/README.md).
