---
title: Import and export
description: Build deployment plans that export content, settings and configuration, download or send them, import packages and JSON, and export content items from the content list.
technical_manual:
  - reference/modules/Deployment/README.md
  - reference/modules/Deployment.Remote/README.md
  - reference/modules/Recipes/README.md
  - reference/modules/Contents/README.md
---

# Import and export

Import and export moves content and configuration from one site to another: from a test site to the live site, between tenants, or into a backup file. You describe what to export in a **deployment plan**, a list of **steps** such as "all content of these types", "the site settings" or "all roles". When you run the plan, Orchard Core writes a **package**, a `.zip` file with a `Recipe.json` file inside, that another site can import.

| | |
| --- | --- |
| **Menu** | **Tools > Deployments > Plans**, **Package Import** and **JSON Import**; **Settings > Deployment Targets** |
| **Permission** | *Manage deployment plans* and *Export Data*, both, to open **Plans**, create and change plans and run them. *Import Data* to import. Administrators have them by default. |
| **Feature** | **Deployment** (`OrchardCore.Deployment`) |


!!! warning "Importing changes the site right away"
    An import runs the package as a [recipe](recipes.md): it adds or replaces content, settings and configuration as soon as you click **Import**, with no preview and no undo. *Import Data* is marked as a security-critical permission. Import only packages you trust, and try them on a test site first.

To send a plan straight to another site instead of downloading a file, see [Remote deployment](remote-deployment.md). For a step-by-step example, see [Copy content between sites](../use-cases/copy-content-between-sites.md).

## Create a deployment plan

1. Open **Tools > Deployments > Plans** and click **Add Deployment Plan**.
2. Type a **Name**. It is required and must be unique.
3. Click **Create**.
4. Click **Manage Steps** in the plan's row.
5. Click **Add Step**. The **Available Steps** window lists every step, grouped by category on the left. Type in **Filter** to find one.
6. Click **Add** on the step you want, fill in its options if it has any, and click **Create**.
7. Repeat for each step.

On the plan's screen, each step shows a summary with **Edit** and **Delete** buttons. Drag a step by its handle to change the order: steps run in that order on import, so put content type definitions before the content that uses them.

## Steps you can add

The steps offered depend on the features turned on in your site; each feature adds the steps for its own data. These are the steps of the main features:

### Content

| Step | What it exports |
| --- | --- |
| **All Content** | The published version of every content item. Option: **Export As Setup recipe**. |
| **Content** | The content items of the content types you tick. Option: **Export As Setup recipe**. |
| **Content Item** | One content item, by its **Content Item Id**. The **Add to Deployment Plan** action of the content list adds this step for you. |
| **Queried Content Items** | The content items returned by a saved **Query**, with optional **Parameters** as JSON. The query must be set to return documents. Option: **Export As Setup recipe**. See [Queries](../search/queries.md). |
| **Update Content Definitions** | The content types and parts you tick (or all of them with **Include all content types and parts definitions.**), merged into the target site's definitions. |
| **Replace Content Definitions** | The same, but replacing the target site's definitions of those types and parts. |
| **Delete Content Definitions** | Deletes the content types and parts you tick on the target site. |
| **Media** | The media files you tick, or all of them with **Include all media.**. The files are added to the package. |
| **All Media Profiles** | Every media profile. |
| **All Layers** | Every layer and its rules. |
| **All Queries** | Every saved query. |
| **All Sitemaps** | Every sitemap. |
| **Admin Menus** | Every custom admin menu. |
| **All Shortcode Templates** | Every shortcode template. |
| **Export Content To Deployment Target** | The content items chosen in the content list, see [Export content items from the content list](#export-content-items-from-the-content-list). |

**Export As Setup recipe** writes the content so it can be used in a setup recipe. Links between content items, such as Content Picker, Taxonomy and Tags fields and list items, are lost and a developer has to fix them by hand, so leave it unticked for an ordinary copy.

### Design and development

| Step | What it exports |
| --- | --- |
| **Themes** | The selected site theme and admin theme. |
| **All Templates** | Every template. |
| **All Admin Templates** | Every admin template. |
| **All Placements** | Every placement defined in **Design > Placements**. |

### Users, security and settings

| Step | What it exports |
| --- | --- |
| **All Roles** | Every role and its permissions. |
| **All Users** | Every user account. |
| **Custom User Settings** | All or some custom user settings. |
| **Site Settings** | The general site settings you tick, such as the site name, time zone, page size or home page. |
| **Custom Settings** | All or some custom settings. |
| **All Features** | Which features are on and off. Option: **Ignore disabled features**, to export only the features that are on. |
| **All Feature Profiles** | Every tenant feature profile. |
| **All Rate Limit Policies** | Every rate limit policy. |
| **Secrets** | Every secret. Without an **Encryption key**, only the names and details of the secrets are exported; with one, their values are exported encrypted, and the target site needs the same key to read them. |
| **... settings** | One step for each settings page, for example **Admin settings**, **Audit Trail settings**, **Cors settings**, **Culture settings**, **Login settings**, **Registration settings**, **Reset Password settings**, **Security settings**, **Reverse Proxy settings**, **ReCaptcha settings**, **Google Analytics Settings**, **Microsoft Entra ID Settings**, **OpenID Server Settings** and others. |

### Search, languages, workflows and data

| Step | What it exports or does |
| --- | --- |
| **Index Profiles** | All or some search index definitions. |
| **Lucene Search Indexes**, **Elasticsearch Search Indexes**, **Azure AI Search Indexes** | All or some indexes of that search engine. |
| **Rebuild Indexes**, **Reset Indexes** and their Lucene, Elasticsearch and Azure AI Search versions | Rebuilds or resets the chosen indexes on the target site after the import, see [Site search](../search/site-search.md). |
| **Search Settings** | The site search settings. |
| **Translations** | The translations of the cultures and categories you choose, see [Localization](../content/localization.md). |
| **Dynamic Data Translations** | Every dynamic data translation. |
| **All Workflows** | Every workflow. |
| **Data Pipelines** | All or some data pipelines, without the secrets of their steps. |

### Deployment

| Step | What it does |
| --- | --- |
| **Deployment Plans** | All or some deployment plans, so the target site gets the same plans. |
| **Recipe File** | Sets the name, description, author and other details written at the top of `Recipe.json`. Tick **Setup recipe** when a developer will use the package as a setup recipe. |
| **JSON Recipe** | Adds recipe steps that you paste as JSON. Ask a developer for the JSON. |
| **Custom File** | Adds a file to the package: type its **File Name** and **File Content**. |

## Run a plan and download the package

1. Open **Tools > Deployments > Plans**.
2. Click **Manage Steps** in the plan's row.
3. Click **Execute**.
4. In **Available Targets**, click **Select** under **File Download**.

Your browser downloads a `.zip` file named after the plan. It contains `Recipe.json` and any files the steps added, such as media. The other targets in the window are remote sites, see [Remote deployment](remote-deployment.md).

## Manage plans

In **Tools > Deployments > Plans**, each row has **Manage Steps**, **Edit** (to rename the plan) and **Delete**. Type in **Search** to filter the list. To delete several plans, tick them and choose **Delete** in the **Actions** menu above the list.

## Import a package

1. Open **Tools > Deployments > Package Import** on the site that receives the data.
2. Click **Select a file** and choose the `.zip` package, or a single `.json` recipe file.
3. Click **Import**.

You see *Deployment package imported.* when it worked. If a step fails, you see *The import failed with the following errors: ...*. A step that needs a feature that isn't turned on in this site is skipped without a message, so turn on the same features as the source site first, or include the **All Features** step.

## Import JSON

To import a recipe without a file, for example steps a developer sent you:

1. Open **Tools > Deployments > JSON Import**.
2. Paste the recipe into **JSON**. It is required.
3. Click **Import**.

You see *Recipe imported successfully!*. If the text isn't valid JSON, you see *The recipe is written in an incorrect JSON format.* and nothing is imported.

## Export content items from the content list

Three features add export actions to **Content > Content Items**, see [Content items](../content/content-items.md):

| Feature | Actions it adds |
| --- | --- |
| **Add Content To Deployment Plan** (`OrchardCore.Contents.Deployment.AddToDeploymentPlan`) | **Add to Deployment Plan** in a row's **Actions** menu, and **Add to deployment plan** in the bulk **Actions** menu. Pick the plan in **Available Deployment Plans** and click **Select**. A **Content Item** step is added to the plan for each item. Needs *Manage deployment plans* and *Export Data*, and permission to edit the items. |
| **View Or Download Content As JSON** (`OrchardCore.Contents.Deployment.Download`) | **View as JSON** and **Download as JSON** in a row's **Actions** menu. When the item has a draft, you get **View Draft as JSON**, **Download Draft as JSON**, **View Published as JSON** and **Download Published as JSON**. The view page has a **Download** button. Needs *Export Data* and permission to edit the item. |
| **Export Content To Deployment Target** (`OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget`) | **Export to Deployment Target** in a row's **Actions** menu (**Export Draft to Deployment Target** and **Export Published to Deployment Target** when the item has a draft), and **Export to deployment target** in the bulk **Actions** menu. Pick a target in **Available Targets** and click **Select**: **File Download** downloads a package, a remote instance receives it directly. The bulk action exports the published versions only. Needs *Export Data*. |

### Choose the plan used by Export to Deployment Target

**Export to Deployment Target** runs a deployment plan that contains the **Export Content To Deployment Target** step, with the items you chose. Turning the feature on creates a plan named **Export content to deployment plan target** for this. You can add other steps to that plan, for example the content definitions.

To use another plan:

1. Create the plan and add the **Export Content To Deployment Target** step to it.
2. Open **Settings > Deployment Targets**.
3. Pick the plan in **Deployment Plan**.
4. Click **Save**.

If the page says *No export content to deployment target plans found. Create a deployment plan and add a Export content to deployment target step to it.*, add that step to a plan first. The page needs *Manage deployment plans*.

!!! example "See it in practice"
    [Copy content between sites](../use-cases/copy-content-between-sites.md) walks through this step by step.
