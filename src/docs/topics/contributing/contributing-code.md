# Contributing code to Orchard Core

First of all, thank you for thinking about contributing to the code! Be sure to check out [the general contribution docs](README.md) first.

Are you looking for information on contributing documentation? Head over to [here](contributing-documentation.md) instead.

## Working with Orchard Core's source code

First, clone the repository using the command `git clone https://github.com/OrchardCMS/OrchardCore.git` and checkout the `main` branch. Then, you have multiple options, see below.

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

- Check out [the issue tracker](https://github.com/OrchardCMS/OrchardCore/issues) for open bug reports and feature requests. Prefer to select issues [scheduled for the upcoming release](https://github.com/OrchardCMS/OrchardCore/milestones).
- ["Good first issue" issues](https://github.com/OrchardCMS/OrchardCore/labels/good%20first%20issue): We think these are a good for newcomers.
- ["Help wanted" issues](https://github.com/OrchardCMS/OrchardCore/labels/help%20wanted): These issues are up for grabs. Comment on an issue if you want to create a fix.

## Identifying the scale

First identify the scale of what you would like to contribute. If it is small (grammar/spelling or a bug fix) feel free to start working on a fix. If you are submitting a feature or other substantial code contribution, please discuss it with the team first and ensure it follows the roadmap and fits into the bigger picture. We'd hate to see your work go to waste! The best is if you work on open issues, and open an issue for your idea too, before starting coding.

You might also read these two blogs posts on contributing code: [Open Source Contribution Etiquette](http://tirania.org/blog/archive/2010/Dec-31.html) by Miguel de Icaza and [Don't "Push" Your Pull Requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/) by Ilya Grigorik.

## Submitting a pull request

If you don't know what a pull request is read [this article](https://help.github.com/articles/using-pull-requests). Make sure the repository can build and all tests pass. Familiarize yourself with the project and our coding conventions, as you can see in the repository. We follow the [ASP.NET Core Engineering guidelines](https://github.com/dotnet/aspnetcore/wiki/Engineering-guidelines). When in doubt, just as the community for guidance.

## Feedback

All code submissions will be reviewed and tested by the core team, and only those that meet our bar for both quality and design/roadmap appropriateness will be merged. Please be patient; we very much appreciate you contributing to Orchard Core!

Please update your pull request according to feedback until it is approved by one of the core team members.

