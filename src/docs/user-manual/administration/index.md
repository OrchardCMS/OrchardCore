---
title: Administration
description: Run the site behind the scenes - turn features on, set up sites and tenants, run recipes, move content between sites, read the audit trail, and look after background tasks, redirects, secrets and developer tools.
---

# Administration

This section is for the people who run the site behind the scenes: they decide which features are on, set up new sites, copy content and settings from one site to another, and keep an eye on what happens. It is mostly for **administrators**. Some pages, such as developer tools and secrets, are best handled together with your developer or IT team.

Most of these screens are under **Tools** in the admin menu: **Tools > Features**, **Tools > Recipes**, **Tools > Deployments**, **Tools > Audit Trail**, **Tools > Background Tasks**, **Tools > URL Rewriting**, **Tools > Admin Menus** and **Tools > Security** (secrets). The dashboard is the first page of the admin. Tenants are under **Multi-Tenancy**. A new site or tenant shows its own setup screen.

## I want to…

| I want to… | Read |
| --- | --- |
| Turn a feature on or off | [Features](../getting-started/features.md#turn-a-feature-on) |
| Find which page of this manual covers a feature | [Features](../getting-started/features.md#which-page-covers-each-feature) |
| Set up a new site and choose a starter recipe | [Set up a site](../getting-started/setting-up-a-site.md#choose-a-recipe) |
| Run a second, separate site from the same installation | [Tenants](tenants.md#add-a-tenant) |
| Limit the features a tenant may turn on | [Tenants](tenants.md#limit-the-features-of-a-tenant) |
| Add features, content or settings in one step with a recipe | [Recipes](recipes.md#run-a-recipe) |
| Export content and settings to a file, and import it on another site | [Import and export](import-and-export.md#create-a-deployment-plan) |
| Send content straight from a staging site to the live site | [Remote deployment](remote-deployment.md#send-a-plan-to-another-site) |
| See who changed a page, and restore an earlier version | [Audit trail](../content/audit-trail.md#restore-an-earlier-version) |
| Run a scheduled job now, or change its schedule | [Background tasks](../settings/background-tasks.md#run-a-task-now) |
| Send visitors from an old address to a new one | [URL rewriting](../settings/url-rewriting.md#add-a-redirect-rule) |
| Keep a password or API key in one safe place | [Secrets](../settings/secrets.md#add-a-secret) |
| Add a shortcut to a content list in the admin menu | [Admin menus](../design/admin-menu.md#create-an-admin-menu) |
| Put widgets on the admin dashboard | [Admin dashboard](../design/dashboard.md#build-a-dashboard-of-widgets) |
| Explore the site's content and APIs with GraphiQL or OpenAPI | [Developer tools](../settings/developer-tools.md) |

## Pages in this section

- [Features](../getting-started/features.md): turn features on and off, dependencies, and the page for every feature.
- [Set up a site](../getting-started/setting-up-a-site.md): the setup screen, the starter recipes and the super user.
- [Tenants](tenants.md): several separate sites from one installation, and feature profiles.
- [Recipes](recipes.md): the recipes your site offers, and running one.
- [Import and export](import-and-export.md): deployment plans, package and JSON import, and exporting from the content list.
- [Remote deployment](remote-deployment.md): send deployment plans straight from one site to another.
- [Audit trail](../content/audit-trail.md): the history of changes, comparing versions and restoring items.
- [Background tasks](../settings/background-tasks.md): the jobs the site runs on a schedule.
- [URL rewriting](../settings/url-rewriting.md): redirect and rewrite rules.
- [Secrets](../settings/secrets.md): passwords, keys and certificates kept in one place.
- [Admin menus](../design/admin-menu.md): your own entries in the admin menu, and who sees them.
- [Admin dashboard](../design/dashboard.md): the welcome page of the admin and its widgets.
- [Developer tools](../settings/developer-tools.md): GraphiQL, OpenAPI, the Mini Profiler, health checks and features IT sets up.

**See it in practice:** [Copy content between sites](../use-cases/copy-content-between-sites.md).
