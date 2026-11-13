---
title: Recipes
description: See the recipes your site offers, know what the built-in ones do, and run one to add features, content or configuration in one step.
technical_manual:
  - reference/modules/Recipes/README.md
  - getting-started/starter-recipes.md
---

# Recipes

A recipe is a file that changes a site in one go: it can turn on features, add content types and content, create roles, indexes, queries or workflows, and change settings. Developers and modules ship recipes for common set-ups. Setup recipes run once, when a site is set up (see [Setting up a site](../getting-started/setting-up-a-site.md)). The other recipes are listed in **Tools > Recipes**, where you can run them on a working site.

| | |
| --- | --- |
| **Menu** | **Tools > Recipes** |
| **Permission** | *Manage Recipes*. Administrators have it by default. |
| **Feature** | **Recipes** (`OrchardCore.Recipes`), always on. It uses **Recipes Core Services** (`OrchardCore.Recipes.Core`), turned on for you. |


!!! warning "Recipes change the site at once"
    Running a recipe applies all its changes immediately: it can replace content, settings and other configuration, and there is no undo. *Manage Recipes* is marked as a security-critical permission. Read the recipe's description first, and try it on a test site.

## Find a recipe

Open **Tools > Recipes**. The recipes are grouped under the name of the feature or theme that ships them, and each one shows its name, its tags and a short description. Type in **Search** to filter the list by recipe name or group name. The layout button switches between the list and grid views, see [Finding your way](../getting-started/finding-your-way.md).

The page lists the recipes of every module and theme of your installation, even when the feature or theme isn't turned on. It doesn't list setup recipes, nor recipes marked as hidden.

## Run a recipe

1. Open **Tools > Recipes**.
2. Find the recipe and click **Run**.
3. Click **Yes** when asked *Are you sure you want to run the recipe?*.

When it finishes, you see *The recipe '...' has been run successfully.* and the site reloads its configuration, so the next page can take a moment longer. If a step fails, you see *The recipe '...' failed to run due to the following errors: ...* followed by the errors; steps that ran before the error are not undone.

## Recipes that ship with Orchard Core

Your list may contain more, from other modules. These come with Orchard Core:

| Group | Recipe | What it does |
| --- | --- | --- |
| **Admin** | **Admin Dashboard Widget Sample** | Adds a sample widget to the admin dashboard, see [Dashboard](../design/dashboard.md). |
| **Menu** | **Add Permissions to Menus** | Adds the menu item permission part to the **Link Menu Item** and **Html Menu Item** content types, so a menu item can be shown only to people with a given permission. Use it when your menu items don't offer that option, see [Menus](../design/menus.md). |
| **The Blog Theme** | **Blog - Lucene Search** | Turns on **Search** and **Lucene**, creates a Lucene index named **Search** for the blog's content types, makes it the default search index, builds it, and lets anonymous visitors search it. See [Site search](../search/site-search.md). |
| **The Blog Theme** | **Blog - Lucene Query** | Runs **Blog - Lucene Search**, then replaces the blog's **RecentBlogPosts** query with a Lucene query. See [Queries](../search/queries.md). |
| **The Blog Theme** | **Blog - Elasticsearch Search** | The same as **Blog - Lucene Search** with Elasticsearch. Needs the Elasticsearch connection set up by IT. |
| **The Blog Theme** | **Blog - Elasticsearch Query** | Runs **Blog - Elasticsearch Search** and replaces **RecentBlogPosts** with an Elasticsearch query. |
| **The Blog Theme** | **Blog - SQL Search** | Turns on **SQL Queries**. |
| **The Blog Theme** | **Blog - SQL Query** | Runs **Blog - SQL Search** and replaces **RecentBlogPosts** with a SQL query. |
| **Workflows** | **Workflow Variables Samples** | Adds sample workflows that show how workflow variables work, see [Workflows](../forms-and-automation/workflows.md). |
| **OpenApi** | **OpenAPI Documentation — Bearer/PKCE** | For developers. Sets up the OpenID Connect server so the API documentation pages can call the API. |
| **OpenApi** | **OpenApi Generation** | For developers. Turns on every feature that has an API and allows reading the API description without signing in. |
| **Secure Media** | **Media API — Bearer/PKCE** | For developers. Sets up the OpenID Connect server for the media library's API. |
| **Secure Media** | **Media API — Standalone external app** and **Media API — Standalone external app (localhost dev)** | For developers. Let a separate media gallery application call the media API from another address. |

The **Blog** recipes are meant for sites set up with the **Blog** recipe, which has the blog content types and the **RecentBlogPosts** query.

!!! warning "Developer recipes"
    The OpenAPI and Media API recipes contain example addresses such as `https://localhost:5001`, and their descriptions tell developers to replace them first. Running them unchanged on a live site configures the wrong addresses and changes security settings. Leave them to your developer.

## Run a recipe file you were given

A recipe file that isn't in the list, for example one a developer sent you, can be imported with **Tools > Deployments > Package Import** (a `.json` or `.zip` file) or **Tools > Deployments > JSON Import** (pasted text). See [Import and export](import-and-export.md). Importing runs it exactly like **Run** does.

## Create a recipe

Developers write recipes as files in a module or theme. You can also build one in the admin: a deployment plan with a **Recipe File** step produces a package whose `Recipe.json` is a recipe, see [Import and export](import-and-export.md).
