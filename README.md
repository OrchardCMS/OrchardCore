# Orchard 2

Orchard 2 is a re-implementation of [Orchard CMS](https://github.com/OrchardCMS/Orchard) in [ASP.NET Core](http://www.asp.net/vnext). You can check out the [Orchard 2 presentation from the last Orchard Harvest](https://www.youtube.com/watch?v=TK6a_HfD0O8) to get an introductory overview of its features and goals.

[![Join the chat at https://gitter.im/OrchardCMS/Orchard2](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/OrchardCMS/Orchard2?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![BSD-3-Clause License](https://img.shields.io/badge/license-BSD--3--Clause-blue.svg)](LICENSE.txt)

## Build Status

| Build server| Platform       | Status                                                                                                                                                                  |
|-------------|----------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| AppVeyor    | Windows        | [![Build status](https://ci.appveyor.com/api/projects/status/ilx2dx9wvmm2cjdp/branch/master?svg=true)](https://ci.appveyor.com/project/alexbocharov/orchard2/branch/master) |
| Travis      | Linux / OS X   | [![Build Status](https://travis-ci.org/OrchardCMS/Orchard2.svg?branch=master)](https://travis-ci.org/OrchardCMS/Orchard2)                                                 |
| MyGet       | Windows        | [![brochard MyGet Build Status](https://www.myget.org/BuildSource/Badge/brochard?identifier=098718e3-f53d-4bcd-b29e-cb9da86823c0)](https://www.myget.org/)              |

## Orchard CMS

Orchard is a free, [open source](https://github.com/OrchardCMS/Orchard), community-focused Content Management System built on the ASP.NET MVC platform.

## Status

### Pre-Alpha

The software is still under active development and not feature complete or ready for consumption by anyone other than software developers. There may be milestones during the pre-alpha which deliver specific sets of functionality, and nightly builds for other developers or users who are comfortable living on the absolute bleeding edge.

Here is a more detailed [roadmap](https://github.com/OrchardCMS/Orchard2/wiki/Roadmap).

## Getting Started

- Clone the repository using the command `git clone https://github.com/OrchardCMS/Orchard2.git` and checkout the `master` branch. 
- Delete `%LocalAppData%\Microsoft\dotnet` – The shared runtime dotnet installer doesn’t account for the old CLI structure.
- Delete `.build` (or run git clean -xdf) to get the latest KoreBuild
- Delete `C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\dotnet-test-xunit`
- Run the `build.cmd` file included in the repository to dotnet CLI and build the solution.
- Next navigate to `D:\Orchard2\src\Orchard.Cms.Web` or wherever your respective folder is on the command line in Administrator mode.

### Using Kestrel

- Call `dotnet run`.
- Then open the `http://localhost:5000` URL in your browser.

### Using Console

- Call `dotnet run`.
- From here you can now execute commands in a similar fashion as before.

### Contributing

We currently follow the these [engineering guidelines](https://github.com/OrchardCMS/Orchard2/wiki/Engineering-Guidelines).
