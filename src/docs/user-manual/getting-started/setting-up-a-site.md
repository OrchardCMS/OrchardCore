---
title: Set up a site
description: Fill in the setup screen of a new site or tenant, choose a starter recipe, and create the super user account.
technical_manual:
  - reference/modules/Setup/README.md
  - reference/modules/AutoSetup/README.md
  - getting-started/starter-recipes.md
  - reference/modules/Recipes/README.md
---

# Set up a site

A new Orchard Core site, or a new tenant, starts with a **Setup** screen. You answer a few questions once: the name of the site, the starter recipe, the time zone, the database, and the first user account. When you click **Finish Setup**, Orchard Core creates the database tables, runs the recipe and opens the new site.

| | |
| --- | --- |
| **Menu** | The **Setup** page, shown at the address of a site or tenant that isn't set up yet |
| **Permission** | None. Anyone who can reach the page can fill it in, so set up a site as soon as it is installed. A tenant's setup page needs the link from the tenant list (see below). |
| **Feature** | **Setup** (`OrchardCore.Setup`). It is only active on a site that isn't set up yet, so **Tools > Features** shows it as disabled afterward. |

!!! note "Who sees this screen"
    The first site of an installation shows the setup screen the first time someone opens it. A tenant shows it after an administrator adds the tenant in **Multi-Tenancy > Tenants**, see [Tenants](../administration/tenants.md).

## Open the setup screen of a tenant

A tenant's setup page is protected by a secret link, so strangers can't set it up.

1. On the main site, open **Multi-Tenancy > Tenants**.
2. Find the tenant. Its row shows **Uninitialized**.
3. Click **Setup** in its row. You can also click **Create &amp; Setup** when you add the tenant.

The link works for 24 hours. When it expires, open the tenant list again to get a fresh link. Opening the tenant's address without the link shows a "not found" page.

## Fill in the setup screen

1. Type the name of your site in **What is the name of your site?**.
2. Pick a **Recipe**. Each recipe shows a short description in the list. See [Choose a recipe](#choose-a-recipe).
3. Check the **Default Time Zone**.
4. Choose the database in **What type of database to use?** and fill in the database fields that appear.
5. Fill in the **Super User** account: **User Name**, **Email**, **Password** and **Password Confirmation**.
6. Click **Finish Setup**.

Setup takes a few seconds to a minute, depending on the recipe. When it is done, the browser opens the home page of the new site. Sign in with the super user name and password, see [Signing in](signing-in.md).

| Field | What it does |
| --- | --- |
| **Change language** | Shown at the top right when the site supports more than one language. Changes the language of the setup screen. |
| **What is the name of your site?** | Required. Used as the default title of your pages. You can change it later in **Settings > General**. |
| **Recipe** | The starter recipe to run, see the next section. The recipe tagged as the default is selected first; on a standard installation that is **Software as a Service**. Hidden when the tenant was created with a recipe already chosen. |
| **Default Time Zone** | The time zone used to show and edit dates and times. It starts with the time zone of the server. You can change it later in **Settings > General**. |
| **What type of database to use?** | Where the site stores its settings and content: **Sql Server**, **Sqlite**, **MySql** or **Postgres**. **Sqlite** is selected by default and needs nothing else: it keeps the data in a file on the server. The other databases need a database server that your IT team provides. |
| **Table Prefix** | Optional, shown for Sql Server, MySql and Postgres. A short word put in front of every table name, so several sites can share one database. |
| **Connection String** | Required for Sql Server, MySql and Postgres. The address and credentials of the database, given by your IT team. The hint under the box shows an example. Click the eye button to show or hide what you typed. |
| **Table Schema** | Optional, shown for Sql Server, MySql and Postgres. The database schema for the tables. Leave it blank to use the server's default, for example `dbo` for SQL Server. |
| **User Name** | Required. The sign-in name of the super user. Use letters, digits and the characters `-` `.` `_` `+`; the screen rejects other characters with an error. |
| **Email** | Required. A valid email address for the super user. |
| **Password** | Required. Click in the box to see the rules. By default it needs at least 6 characters with an uppercase letter, a lowercase letter, a digit and a symbol. A bar under the box shows how strong it is, and the eye button shows what you typed. Your IT team can change the rules. |
| **Password Confirmation** | Required. Type the password again. It must match. |

!!! warning "The super user"
    The super user has all the rights. Use it to finish setting up the site and in an emergency, and create a separate account with the right role for everyday work, see [Users](../users/users.md).

If the database fields are missing, your IT team has already chosen the database for this site or tenant in its configuration, and setup uses it.

### If setup fails

- A message next to a field tells you what to fix, for example *The password confirmation doesn't match the password.* or *The email is invalid.*
- A connection problem, such as a wrong connection string or a database server that can't be reached, is shown at the top of the form.
- *An error occurred while running the setup. Try again in a few minutes, and if the issue persists, consult the app's operator.* means something went wrong on the server. Ask your IT team to check the logs.
- *No recipes available.* means the installation contains no setup recipe. Ask a developer.

## Choose a recipe

A recipe is a ready-made starting point: it turns on a set of features, picks a theme, and can add content types, sample content, roles and settings. You can turn other features on later in **Tools > Features**, see [Features](features.md), and change the theme in **Design > Themes**, see [Themes](../design/themes.md).

Every recipe except **Software as a Service** adds the roles **Moderator**, **Editor**, **Author** and **Contributor**, next to the **Administrator**, **Authenticated** and **Anonymous** roles every site has. The features the recipe turns on give the roles their default permissions. See [Roles](../users/roles.md).

| Recipe | What you get | Use it when |
| --- | --- | --- |
| **Agency** | A one-page company website with **The Agency Theme**. It adds the content types **Landing Page**, **Page**, **Service**, **Project**, **Milestone**, **Team Member** and **Client**, plus the widgets **Html**, **Blockquote**, **Container**, **Image** and **Paragraph**. It creates a **Main Menu** and a landing page named after your site, with sections for services, portfolio, about, team and clients, sample images and templates. Widgets, layers, menus, templates, placements and layer rules are turned on. | You want a ready-to-edit company site. |
| **Blank site** | Content management features (content types, lists, flows, HTML, Markdown, media, menus, widgets, layers, queries, templates, deployment) with the admin theme only. No site theme, no content types and no content. | You build your own theme, or a developer uses Orchard Core as a back end for another website. |
| **Blog** | A working blog with **The Blog Theme**: the content types **Blog**, **Blog Post**, **Article**, **Page**, **Tag** and **Category**, the widgets **Container**, **Blockquote**, **Image**, **Paragraph** and **Raw Html**, a **Main Menu** (Home and About), **Tags** and **Categories** taxonomies, a sample blog post, an About page, a footer, sample images, a **RecentBlogPosts** query, an admin menu, RSS feeds and taxonomies. | You want a blog or a simple content site to start from. |
| **Coming Soon** | A single "coming soon" page with **The Coming Soon Theme**, social links (Twitter, Facebook, Instagram) and a sign-up form. A **User Registration** workflow checks the form, reCAPTCHA included, and registers the visitor as a user. Forms, workflows, email, reCAPTCHA, localization, user registration and tenants are turned on. | You need a placeholder page that collects sign-ups before launch. Email and reCAPTCHA need their settings filled in, see [Email](../settings/email.md) and [Security](../settings/security.md). |
| **Headless site** | Content management with the GraphQL API and the OpenID Connect server (applications, scopes, token validation). No site theme: the home page of the site opens the admin. | Another application (a mobile app, a JavaScript site) reads the content through the API. A developer sets up the API clients, see [OpenID Connect server](../users/openid-server.md). |
| **Software as a Service** | A minimal site with **The Default Theme** that turns on **Tenants**, so you can run several sites from this one, plus localization and the admin. No content types and no content. | You host several sites (tenants) from one installation, see [Tenants](../administration/tenants.md). |

!!! note
    Your installation can contain more recipes, added by developers or by other modules, and the list on your setup screen is the one that counts. Recipes marked as hidden, used by developer tools, never show in the list.

## Set up a site without the setup screen

Your IT team can set up sites automatically from configuration with the **Auto Setup** feature (`OrchardCore.AutoSetup`), without anyone filling in the setup screen. They choose the site name, recipe, database and super user in the configuration. See the Technical Manual.

## Related pages

- [Tenants](../administration/tenants.md): add a tenant and choose its recipe and database ahead of time.
- [Recipes](../administration/recipes.md): run more recipes after setup.
- [General settings](../settings/general.md): change the site name and time zone later.
