# Orchard Core 

Orchard Core consists of two distinct projects:

- __Orchard Core Framework__: An application framework for building modular, multi-tenant applications on ASP.NET Core.
- __Orchard Core CMS__: A Web Content Management System (CMS) built on top of the Orchard Core Framework.

[![Join the chat at https://gitter.im/OrchardCMS/OrchardCore](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/OrchardCMS/OrchardCore?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![BSD-3-Clause License](https://img.shields.io/badge/license-BSD--3--Clause-blue.svg)](LICENSE)
[![Documentation](https://readthedocs.org/projects/orchardcore/badge/)](https://docs.orchardcore.net/)
[![Crowdin](https://badges.crowdin.net/orchard-core/localized.svg)](https://crowdin.com/project/orchard-core)

## Local communities

中文资源

[![Orchard Core CN 中文讨论组](https://docs.orchardcore.net/en/latest/docs/assets/images/orchard-core-cn-community-logo.png)](https://shang.qq.com/wpa/qunwpa?idkey=48721591a71ee7586316604a7a4ee99d26fd977c6120370a06585085a5936f62)

## Build Status

Stable (release/1.7): 

[![Build status](https://github.com/OrchardCMS/OrchardCore/actions/workflows/release_ci.yml/badge.svg)](https://github.com/OrchardCMS/OrchardCore/actions?query=workflow%3A%22Release+-+CI%22)
[![NuGet](https://img.shields.io/nuget/v/OrchardCore.Application.Cms.Targets.svg)](https://www.nuget.org/packages/OrchardCore.Application.Cms.Targets)

Nightly (main): 

[![Build status](https://github.com/OrchardCMS/OrchardCore/actions/workflows/preview_ci.yml/badge.svg)](https://github.com/OrchardCMS/OrchardCore/actions?query=workflow%3A%22Preview+-+CI%22)
[![Cloudsmith](https://api-prd.cloudsmith.io/badges/version/orchardcore/preview/nuget/OrchardCore.Application.Cms.Targets/latest/x/?render=true&badge_token=gAAAAABey9hKFD_C-ZIpLvayS3HDsIjIorQluDs53KjIdlxoDz6Ntt1TzvMNJp7a_UWvQbsfN5nS7_0IbxCyqHZsjhmZP6cBkKforo-NqwrH5-E6QCrJ3D8%3D)](https://cloudsmith.io/~orchardcore/repos/preview/packages/detail/nuget/OrchardCore.Application.Cms.Targets/latest/)

## Status

### 1.7.0

The software is finished -- and by finished, we mean there are no show-stopping, little-children-killing bugs in it. That we know of. There are probably numerous lower-priority bugs triaged into the next point release or service pack, as well.

Here is a more detailed [roadmap](https://github.com/OrchardCMS/OrchardCore/wiki/Roadmap).

## Getting Started

- Clone the repository using the command `git clone https://github.com/OrchardCMS/OrchardCore.git` and checkout the `main` branch.

### Command line

- Install the latest version of the .NET SDK from this page <https://dotnet.microsoft.com/download>
- Next, navigate to `./OrchardCore/src/OrchardCore.Cms.Web`.
- Call `dotnet run`.
- Then open the `http://localhost:5000` URL in your browser.

### Visual Studio

- Download Visual Studio 2022 (v17.5+) from https://www.visualstudio.com/downloads/
- Open `OrchardCore.sln` and wait for Visual Studio to restore all Nuget packages.
- Ensure `OrchardCore.Cms.Web` is the startup project and run it.

### Docker

- Run `docker run --name orchardcms -p 8080:80 orchardproject/orchardcore-cms-linux:latest`

Docker images and parameters can be found at <https://hub.docker.com/u/orchardproject/>  
See [Docker documentation](https://docs.docker.com/engine/reference/commandline/run/#publish-or-expose-port--p---expose) to expose different port.

### Documentation

The documentation can be accessed here: <https://docs.orchardcore.net/>

## Code of Conduct

See [CODE-OF-CONDUCT](./CODE-OF-CONDUCT.md)

## .NET Foundation

This project is supported by the [.NET Foundation](http://www.dotnetfoundation.org).
