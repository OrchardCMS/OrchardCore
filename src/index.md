# Orchard Core

Orchard Core is a redevelopment of [Orchard CMS](https://github.com/OrchardCMS/Orchard) on [ASP.NET Core](https://www.asp.net/vnext). 

Orchard Core consists of two different targets:

- **Orchard Core Framework**: An application framework for building **modular**, **multi-tenant** applications on ASP.NET Core.
- **Orchard Core CMS**: A Web Content Management System (CMS) built on top of the Orchard Core Framework.

It’s important to note the differences between the framework and the CMS. Some developers who want to develop SaaS applications will only be interested in the modular framework. Others who want to build administrable websites will focus on the CMS and build modules to enhance their sites or the whole ecosystem.

[![Join the chat at https://gitter.im/OrchardCMS/OrchardCore](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/OrchardCMS/OrchardCore?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![BSD-3-Clause License](https://img.shields.io/badge/license-BSD--3--Clause-blue.svg)](https://github.com/OrchardCMS/OrchardCore/blob/master/LICENSE)

## Building Software as a Service (SaaS) solutions with the Orchard Core Framework

It’s very important to understand the Orchard Core Framework is distributed independently from the CMS on nuget.org. We’ve made some sample applications on <https://github.com/OrchardCMS/OrchardCore.Samples> that will guide you on how to build **modular** and **multi-tenant** applications using just Orchard Core Framework without any of the CMS specific features.

One of our goals is to enable community-based ecosystems of hosted applications which can be extended with modules, like e-commerce systems, blog engines and more. The Orchard Core Framework enables a modular environment that allows different teams to work on separate parts of an application and make components reusable across projects.

## Building Website with Orchard Core CMS

Orchard Core CMS is a complete rewrite of Orchard CMS on ASP.NET Core. It’s not just a port as we wanted to improve the performance drastically and align as close as possible to the development models of ASP.NET Core.

- **Performance**. This might the most obvious change when you start using Orchard Core CMS. It’s extremely fast for a CMS. So fast that we haven’t even cared about working on an output cache module. To give you an idea, without caching Orchard Core CMS is around 20 times faster than the previous version.

- **Portable**. You can now develop and deploy Orchard Core CMS on Windows, Linux and macOS. We also have Docker images ready for use.

- **Document database **abstraction. Orchard Core CMS still requires a relational database, and is compatible with SQL Server, MySQL, PostgreSQL and SQLite, but it’s now using a document abstraction (YesSql) that provides a document database API to store and query documents. This is a much better approach for CMS systems and helps performance significantly.

- **NuGet Packages**. Modules and themes are now shared as NuGet packages. Creating a new website with Orchard Core CMS is actually as simple as referencing a single meta package from the NuGet gallery. It also means that updating to a newer version only involves updating the version number of this package.

- **Live preview**. When editing a content item, you can now see live how it will look like on your site, even before saving your content. And it also works for templates, where you can browse any page to inspect the impact of a change on templates as you type it.

- **Liquid templates support**. Editors can safely change the HTML templates with the Liquid template language. It was chosen as it’s both very well documented (Jekyll, Shopify, …) and secure.

- **Custom queries**. We wanted to provide a way for developers to access all their data as simply as possible. We created a module that lets you create custom ad-hoc SQL, and Lucene queries that can be re-used to display custom content, or exposed as API endpoints. You can use it to create efficient queries, or expose your data to SPA applications.

- **Recipes**. Recipes are scripts that can contain content and metadata to build a website. You can now include binary files, and even use them to deploy your sites remotely from a staging to a production environment for instance. They can also be part of NuGet Packages, allowing you to ship predefined websites.

- **Scalability**. Because Orchard Core is a multi-tenant system, you can host as many websites as you want with a single deployment. A typical cloud machine can then host thousands of sites in parallel, with database, content, theme and user isolation.

## Status

The latest released version of Orchard Core is `1.0.0-beta2`.
The release notes can be found on <https://github.com/OrchardCMS/OrchardCore/releases/tag/1.0.0-beta2>

The software is complete enough for external testing -- that is, by groups outside the organization or community that developed the software. Beta software is usually feature complete, but may have known limitations or bugs. Betas are either closed (private) and limited to a specific set of users, or they can be open to the general public.

Here is a more detailed [roadmap](https://github.com/OrchardCMS/OrchardCore/wiki/Roadmap).

## Getting Started

- Clone the repository using the command `git clone https://github.com/OrchardCMS/OrchardCore.git` and checkout the `master` branch for the latest release, or the `dev` branch for the cutting-edge version.

- Watch the ASP.NET Community Standup video where Orchard Core was demonstrated: <https://www.youtube.com/watch?v=HeDjv3blBjQ&t=2246s&list=PL1rZQsJPBU2StolNg0aqvQswETPcYnNKL&index=24> 

- Follow the samples on <https://github.com/OrchardCMS/OrchardCore.Samples> that will guide you on how to build **modular** and **multi-tenant** applications

### Command line

- Install the latest versions of the .NET Core SDK from this page <https://www.microsoft.com/net/download/core>
- Call `dotnet build`.
- Next navigate to `D:\OrchardCore\src\OrchardCore.Cms.Web` or wherever your respective folder is on the command line in Administrator mode.
- Call `dotnet run`.
- Then open the `http://localhost:5000` URL in your browser.

You can also read the [Code Generation Templates documentation](Templates/README) to create new applications from predefined templates.

### Visual Studio 2017

- Download Visual Studio 2017 (any edition) from <https://www.visualstudio.com/downloads/>
- Open `OrchardCore.sln` and wait for Visual Studio to restore all Nuget packages
- Ensure `OrchardCore.Cms.Web` is the startup project and run it

### Contributing

We currently follow these [engineering guidelines](https://github.com/OrchardCMS/OrchardCore/wiki/Engineering-Guidelines).
