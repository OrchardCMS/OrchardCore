---
title: Tenants
description: Run several separate sites from one Orchard Core installation, add and set up tenants, disable, reload or remove them, and limit their features with feature profiles.
technical_manual:
  - reference/modules/Tenants/README.md
  - reference/modules/Shells/README.md
  - reference/modules/Setup/README.md
---

# Tenants

A tenant is a separate site that runs inside the same Orchard Core installation. Each tenant has its own address, its own content, users, settings, features and theme, and usually its own database or tables. Use tenants to host several websites, for example one per brand or customer, without installing Orchard Core several times. The first site of the installation, called **Default**, is where you manage all the others.

| | |
| --- | --- |
| **Menu** | **Multi-Tenancy > Tenants**; **Multi-Tenancy > Feature Profiles** |
| **Permission** | *Manage tenants* for tenants. *Manage tenant feature profiles* for feature profiles. Administrators have them by default. |
| **Feature** | **Tenants** (`OrchardCore.Tenants`) and **Tenant Feature Profiles** (`OrchardCore.Tenants.FeatureProfiles`), on the **Default** tenant only |


## How visitors reach a tenant

Each tenant answers on its own address, set by a **Hostname**, a **Url Prefix**, or both:

- **Hostname** only: the tenant is a whole domain, such as `https://brand.example.com/`. Your IT team points the domain name to the server.
- **Url Prefix** only: the tenant lives under the main site's address, such as `https://www.example.com/brand`.
- Both: the tenant answers on that domain and prefix only.

No two tenants can have the same hostname and prefix.

## Add a tenant

1. Open **Multi-Tenancy > Tenants** and click **Add Tenant**.
2. Type the **Name**.
3. Fill in the **Url Prefix**, the **Hostname**, or both.
4. Optionally choose the **Recipe** and the database under **Database Presets**, so whoever sets up the tenant doesn't have to.
5. Click **Create** to set the tenant up later, or **Create &amp; Setup** to open its setup screen right away.

The tenant appears in the list as **Uninitialized** until someone completes its setup, see [Set up a tenant](#set-up-a-tenant).

| Field | What it does |
| --- | --- |
| **Name** | Required. The tenant's unique technical name. Use letters, digits and underscores, with no spaces. It can't be changed later, and it can't be `Default`. |
| **Category** | Optional. A label, such as a customer or region, used to filter the tenant list. |
| **Url Prefix** | Optional. The first part of the path, such as `site1` for `https://www.example.com/site1`. One word only: it can't contain `/`. |
| **Description** | Optional. A note about the tenant, for administrators. |
| **Hostname** | Optional. The domain name of the tenant, such as `site1.example.com`. Separate several domains with commas, and start with `*.` to include every subdomain, such as `*.example.com`. |
| **Recipe** | Optional. The setup recipe to use. When chosen, the tenant's setup screen doesn't ask for it. See [Choose a recipe](../getting-started/setting-up-a-site.md#choose-a-recipe). |
| **What type of database to use?** | Optional. **None** lets the setup screen ask. Otherwise choose **Sql Server**, **Sqlite**, **MySql** or **Postgres** and the setup screen won't ask. |
| **Table Prefix** | For Sql Server, MySql and Postgres. A word put in front of the tenant's table names, so tenants can share one database. Letters, digits and underscores, starting with a letter or underscore. Your IT team can make it required, or have it generated for you; the hint under the box says so. |
| **Connection string** | For Sql Server, MySql and Postgres. The address and credentials of the database, given by your IT team. It is checked when you save. |
| **Table Schema** | For Sql Server, MySql and Postgres. Optional. Leave it blank to use the server's default, for example `dbo` for SQL Server. |
| **Feature profile** | Shown only when at least one feature profile exists. Limits the features the tenant can turn on, see [Limit the features of a tenant](#limit-the-features-of-a-tenant). |

When your IT team has fixed the database for all tenants in the configuration, the database fields show their values read-only, with the password hidden.

Common errors: *A tenant with the same host and prefix already exists.*, *A tenant with the same name already exists.*, *The provided connection string is invalid or server is unreachable.* and *The provided database, table prefix and schema are already in use.*

## Set up a tenant

A new tenant must be set up before it works: someone fills in its setup screen and chooses the first user account.

1. In **Multi-Tenancy > Tenants**, find the tenant with the **Uninitialized** badge.
2. Click **Setup** in its row. The tenant's setup screen opens.
3. Fill it in and click **Finish Setup**. See [Setting up a site](../getting-started/setting-up-a-site.md).

You can also send the setup link to the person who will run the tenant: right-click **Setup** and copy the link. It works for 24 hours.

## Find a tenant

Each row of **Multi-Tenancy > Tenants** shows the tenant's name (a link to its site), its hostname and prefix, and badges for its state, database, recipe and category. To narrow the list:

- Type in **Search** to find a tenant by name, hostname or prefix.
- Use **Category** to show one category.
- Use **State** to show **All states**, **Running**, **Disabled** or **Uninitialized** tenants.
- Use **Sort** to order by **Name** or **State**.

| State | Meaning |
| --- | --- |
| **Uninitialized** | Created but not set up yet. |
| **Initializing** | Setup is running. |
| **Running** | Set up and serving its site. |
| **Disabled** | Turned off: its site doesn't answer. Its data is kept. |
| **Invalid** | Its settings are broken. Ask your IT team to check the logs. |

## What you can do with a tenant

The buttons in a tenant's row depend on its state:

| Button | When | What it does |
| --- | --- | --- |
| **Edit** | Always | Opens the tenant's settings. |
| **View** | Running | Opens the tenant's site in a new tab. |
| **Features** | Running, not Default | Opens **Features** for that tenant, so you can turn its features on or off from the Default tenant, see [Features](../getting-started/features.md). |
| **Setup** | Uninitialized | Opens the tenant's setup screen. |
| **Disable** | Running, not Default | Turns the tenant off after you confirm. Its site stops answering; nothing is deleted. |
| **Enable** | Disabled | Turns the tenant back on. |
| **Reload** | Always | Restarts the tenant, for example after IT changed its configuration. Its visitors may wait a moment on the next request. |
| **Remove** | Disabled or Uninitialized, and allowed by IT | Deletes the tenant after you confirm, see below. |

To disable or enable several tenants, tick them and choose **Disable** or **Enable** in the **Actions** menu above the list. The **Default** tenant can't be disabled.

## Edit a tenant

1. Click **Edit** in the tenant's row.
2. Change the **Category**, **Url Prefix**, **Description**, **Hostname** or **Feature profile**.
3. Click **Save**.

The **Name** can't be changed. The **Recipe** and database presets can be changed only while the tenant is **Uninitialized**.

## Remove a tenant

Removing a tenant deletes it for good: its settings, its database tables, and its files are removed. Removal is off by default; your IT team turns it on in the configuration.

1. **Disable** the tenant first. A tenant that was never set up can be removed while **Uninitialized**.
2. Click **Remove** in its row and confirm.

You see *The tenant '...' was removed, see the log file for more info.* If something can't be deleted, the message names the problem.

!!! warning
    There is no undo. Export the content you want to keep first, see [Import and export](import-and-export.md).

## Limit the features of a tenant

A feature profile is a list of rules that hides features from the tenants that use it, for example to stop customer sites from turning on workflows.

1. Open **Multi-Tenancy > Feature Profiles** and click **Add Feature Profile**.
2. Type a **Name**.
3. Write the **Rules** (see below).
4. Click **Save**, or **Save and continue** in the menu next to it to stay on the page.
5. Edit each tenant that should use it and pick it in **Feature profile**.

The tenant's **Tools > Features** screen then shows only the features the profile allows.

| Field | What it does |
| --- | --- |
| **Name** | Required and unique. The name you pick on the tenant. |
| **Rules** | Required. A list of rules in JSON. Each rule has a **Rule**, `Include` or `Exclude`, and an **Expression**, the id of a feature such as `OrchardCore.Flows`. The editor suggests the values as you type. |

For example, this profile hides the **Flows** feature and every workflow feature:

```json
[
  { "Rule": "Exclude", "Expression": "OrchardCore.Flows" },
  { "Rule": "Exclude", "Expression": "OrchardCore.Workflows*" }
]
```

How the rules work:

- Every feature is allowed unless a rule excludes it.
- In an expression, `*` matches any text and `?` matches one character.
- When several rules match a feature, the last one wins, so you can exclude a group and then include one feature of it.
- A feature is also hidden when a feature it needs is excluded.

You find feature ids in **Tools > Features**, see [Features](../getting-started/features.md). To change or delete a profile, use **Edit** or **Delete** in its row; tick several and choose **Delete** in the **Actions** menu to delete them together.

## Copy feature profiles to another site

Add the **All Feature Profiles** step to a deployment plan, see [Import and export](import-and-export.md).

## Serve files for each tenant

**Static File Provider** (`OrchardCore.Tenants.FileProvider`) gives each tenant a folder of its own on the server for plain files, such as a `robots.txt` or a site verification file, served at the tenant's address. Your IT team puts the files in the tenant's folder on the server. It can be turned on in any tenant and has no screen.

## Run tenants on several servers

**Distributed Tenants** (`OrchardCore.Tenants.Distributed`) keeps tenants in sync when the site runs on several servers: when a tenant is added, changed or reloaded on one server, the others follow. It needs a shared cache, such as Redis, and other hosting set-up by your IT team. See the Technical Manual.

## Automate tenants

With workflows, you can create, set up, enable and disable tenants automatically, for example when a customer signs up. See the [Tenant activities](../forms-and-automation/workflow-activities.md#tenant).
