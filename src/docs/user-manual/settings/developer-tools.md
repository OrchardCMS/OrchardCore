---
title: Developer tools
description: The GraphQL explorer, the OpenAPI documentation pages, the Mini Profiler, health checks, remote publishing, and the hosting features your IT team sets up.
technical_manual:
  - reference/modules/Apis.GraphQL/README.md
  - reference/modules/OpenApi/README.md
  - reference/modules/MiniProfiler/README.md
  - reference/modules/HealthChecks/README.md
  - reference/modules/XmlRpc/README.md
  - reference/modules/SignalR/README.md
  - reference/modules/Redis/README.md
  - reference/modules/DataProtection.Azure/README.md
  - reference/modules/Logging.Serilog/README.md
  - reference/modules/OpenTelemetry/README.md
---

# Developer tools

Some features are mostly for developers, integrators and power users: tools to explore the site's APIs, to measure how fast pages are, and to check that the site is healthy. Others have no screen at all: they connect the site to hosting services and are set up by your IT team. This page explains what each one does for your site, so you know what you are turning on.

| Tool | Menu | Permission | Feature |
| --- | --- | --- | --- |
| GraphQL explorer | **Tools > GraphiQL** | *Execute GraphQL.* | **GraphQL** (`OrchardCore.Apis.GraphQL`) |
| OpenAPI settings | **Settings > OpenApi** | *Manage OpenAPI settings and access interactive documentation UIs* | **OpenApi** (`OrchardCore.OpenApi`) |
| Mini Profiler | No menu: a box on the pages | *View Mini Profiler widget on front end pages*, *View Mini Profiler widget on back end pages* | **Mini Profiler** (`OrchardCore.MiniProfiler`) |
| Health checks | No screen | | **Health Checks** (`OrchardCore.HealthChecks`) |
| Remote publishing | No screen | | **XML-RPC** (`OrchardCore.XmlRpc`), **Remote Publishing** (`OrchardCore.RemotePublishing`) |

The Administrator role has these permissions by default.


## Explore your content with GraphiQL

GraphQL is a way for other applications, such as a mobile app or a separate website front end, to ask the site for exactly the content they need. **Tools > GraphiQL** opens GraphiQL, an explorer where you type a query on the left and see the answer on the right. The explorer suggests the content types, fields and parts of your site as you type, and its documentation panel lists everything you can ask for.

1. Open **Tools > GraphiQL**.
2. Type a query in the left pane, for example `{ article { displayText } }` to list the titles of your articles.
3. Click the run button (the triangle) at the top.

The answer contains only the content your account is allowed to see. Changing data through GraphQL (mutations) needs the *Execute GraphQL Mutations.* permission. Other applications send their queries to the address `/api/graphql` of your site; your developer sets up how they sign in.

## Browse the API documentation (OpenAPI)

The site describes its web APIs in the OpenAPI format, which tools and developers use to understand and call them. Three optional features show this description as a readable page:

| Feature | Page | What it is |
| --- | --- | --- |
| **OpenApi Swagger UI** (`OrchardCore.OpenApi.SwaggerUI`) | `/swagger` | An interactive explorer: try each API call from the browser. |
| **OpenApi ReDoc UI** (`OrchardCore.OpenApi.ReDocUI`) | `/redoc` | A read-only reference. |
| **OpenApi Scalar UI** (`OrchardCore.OpenApi.ScalarUI`) | `/scalar/v1` | A modern, interactive reference. |

Type the page address after your site's address, for example `https://www.example.com/swagger`. You must be signed in, and your role needs the *Access view content endpoints* permission of the **OpenApi** feature (roles with *Manage OpenAPI settings and access interactive documentation UIs* have it); otherwise you are sent to sign in, or refused.

### Change the OpenAPI settings

1. Open **Settings > OpenApi**.
2. Under **API Documentation UIs**, check which documentation pages are **Enabled** or **Disabled**. To turn one on or off, use **Tools > Features**.
3. Under **API Schema Access**, check **Allow anonymous access to the API schema** only if outside tools must read the API description without signing in. It is off by default.
4. Click **Save**. The site reloads.

## Measure page speed with the Mini Profiler

When the **Mini Profiler** feature is on, people whose role has the permission see a small box in a corner of each page with the time the page took to build. Click it to see the details: which steps, templates and database queries took the time. Visitors without the permission don't see it.

Grant *View Mini Profiler widget on front end pages* to see it on the site, and *View Mini Profiler widget on back end pages* to see it in the admin. Turn the feature off when you are done.

## Health checks

The **Health Checks** feature gives the site an address, `/health/live` by default, that monitoring tools and hosting platforms call to check that the site is up. It has no screen. Your IT team chooses the address and whether it shows details, in the site configuration.

## Publish from a desktop blogging app (XML-RPC)

The **Remote Publishing** feature (which turns on **XML-RPC**) lets people write and publish blog posts from a desktop blogging application that supports the MetaWeblog standard, such as Open Live Writer, instead of the admin.

1. Make sure the feature is on.
2. In the blogging application, add a blog account with the address of your blog (the page of the blog's list), and the user name and password of a site account that is allowed to edit the blog's posts.
3. The application finds the site's publishing address by itself. Write a post and publish it.

Uploading images from the application needs the media permissions of [Media](../media/media-library.md).

## Features your IT team sets up

These features have no screen in the admin. They connect the site to services of the hosting environment, and they need configuration files or settings on the server. Turn them on only when your developer or IT team asks; see the Technical Manual.

| Feature | What it does for your site |
| --- | --- |
| **SignalR** (`OrchardCore.SignalR`) | Live updates between the server and open browser pages, for example the **Media SignalR** feature refreshes the media library for everyone when someone uploads a file. **SignalR Redis Backplane** and **SignalR Azure Backplane** make it work when the site runs on several servers. |
| **Redis** (`OrchardCore.Redis`), **Redis Cache**, **Redis Bus**, **Redis Lock** | Connects the site to a Redis server, a fast shared memory service. Used when the site runs on several servers, so they share their cache, their messages and their locks. |
| **Distributed Data Protection (Azure Storage)** (`OrchardCore.DataProtection.Azure`), **Distributed Data Protection (Redis)** (`OrchardCore.Redis.DataProtection`) | Keeps the keys that protect sign-in cookies and encrypted settings (such as the SMTP password) in a shared place, so every server of the site can read them. Without it, on several servers, people may be signed out and encrypted settings may stop working. |
| Logging (Serilog, NLog) | Writes the site's log files, which IT reads to understand errors. This is part of how the site is installed, not a feature to turn on. |
| OpenTelemetry | Sends the site's traces, metrics and logs to a monitoring service. This is part of how the site is installed, not a feature to turn on. |

For the error pages visitors see, and the debugging options of the admin, see [General settings](general.md).
