# Orchard Core 

Orchard Core is a re-implementation of [Orchard CMS](https://github.com/OrchardCMS/Orchard) in [ASP.NET Core](http://www.asp.net/vnext). You can check out the [Orchard Core presentation from the last Orchard Harvest](https://www.youtube.com/watch?v=TK6a_HfD0O8) to get an introductory overview of its features and goals.

[![Join the chat at https://gitter.im/OrchardCMS/Orchard2](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/OrchardCMS/Orchard2?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![BSD-3-Clause License](https://img.shields.io/badge/license-BSD--3--Clause-blue.svg)](https://github.com/OrchardCMS/Orchard2/blob/master/LICENSE.txt)

## Build Status

[![Build Status](https://img.shields.io/travis/OrchardCMS/Orchard2.svg?label=travis-ci&branch=master&style=flat-square)](https://travis-ci.org/OrchardCMS/Orchard2/branches)
[![Build status](https://img.shields.io/appveyor/ci/alexbocharov/orchard2/master.svg?label=appveyor&style=flat-square)](https://ci.appveyor.com/project/alexbocharov/orchard2/branch/master)

## Orchard CMS

Orchard is a free, [open source](https://github.com/OrchardCMS/Orchard), community-focused Content Management System built on the ASP.NET MVC platform.

## Status

### Alpha

The software is complete enough for internal testing. This is typically done by people other than the software engineers who wrote it, but still within the same organization or community that developed the software. 

Here is a more detailed [roadmap](https://github.com/OrchardCMS/Orchard2/wiki/Roadmap).

## Getting Started

- Clone the repository using the command `git clone https://github.com/OrchardCMS/Orchard2.git` and checkout the `master` branch. 

### Command line

- Install the latest versions (current) for both Runtime and SDK of .NET Core from this page https://www.microsoft.com/net/download/core
- Call `dotnet restore`.
- Call `dotnet build`.
- Next navigate to `D:\Orchard2\src\Orchard.Cms.Web` or wherever your respective folder is on the command line in Administrator mode.
- Call `dotnet run`.
- Then open the `http://localhost:5000` URL in your browser.

### Visual Studio 2017

- Download Visual Studio 2017 (any edition) from https://www.visualstudio.com/downloads/
- Open `Orchard.sln`
- Ensure `OrchardCore.Cms` is the startup project and run it

### Contributing

We currently follow the these [engineering guidelines](https://github.com/OrchardCMS/Orchard2/wiki/Engineering-Guidelines).
