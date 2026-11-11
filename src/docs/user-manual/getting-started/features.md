---
title: Features
description: Turn a feature on or off in Tools > Features, understand dependencies, and find the User Manual page for every feature.
technical_manual:
  - reference/modules/Features/README.md
  - reference/README.md
---

# Features

An Orchard Core site is made of **features**: pieces you turn on only when your site needs them, such as **Forms**, **Workflows** or **Media**. A feature's menus, screens, content parts and permissions appear only while it is on. This page is for **administrators**: it shows how to turn features on and off, and lists every feature with the page of this manual that explains it.

| | |
| --- | --- |
| **Menu** | **Tools > Features** |
| **Permission** | *Manage Features* |
| **Feature** | **Features** (`OrchardCore.Features`), always on |

!!! tip "Don't see this menu?"
    Only people whose role has the *Manage Features* permission see **Tools > Features**. Usually that's the **Administrator** role. See [Roles and permissions](roles-and-permissions.md).

## Find a feature

1. Open **Tools > Features**.
2. Type part of the feature's name, its technical name or a word of its description in the **Search** box. The list narrows as you type, and **Showing ... of ... features** tells you how many match.
3. Use the **Status** menu to show **Any status**, **Enabled only** or **Disabled only**.

Features are grouped by category, such as **Content Management**, **Security** or **Search**. Each row shows the feature's name, its technical name (for example `OrchardCore.Forms`), a badge saying whether it is **Enabled** or **Disabled**, and a short description.

The **Visibility** menu hides two kinds of features by default:

- **Hide always enabled**: features that are part of the site's foundation and can't be turned off.
- **Hide on demand**: features that never need to be turned on by hand, because they turn on automatically when another feature needs them.

Clear these options to see every feature.

Tick **Show indirect dependencies** to see, under each feature, not only the features it needs directly but also the ones those need in turn.

## Turn a feature on

1. Open **Tools > Features** and find the feature.
2. Click **Enable** on its row.

The features it depends on are turned on with it. A message at the top of the page confirms which features were enabled. The feature's menus and screens appear right away; reload the page if the menu doesn't show them yet.

After you turn a feature on, give the right roles its permissions in **Access Control > Roles**. See [Roles](../users/roles.md).

## Turn a feature off

1. Open **Tools > Features** and find the feature.
2. Click **Disable** on its row.
3. Confirm. If other enabled features depend on it, the message lists them: they are turned off too.

Turning a feature off hides its menus and screens. The content and settings it created are kept, so turning it on again brings them back.

!!! warning
    Some features change how the whole site works, such as **HTTPS** (which sends visitors to the secure address of the site) or the sign-in features. Turn them on or off only when you know what they do, and preferably on a test site first.

Some features show a disabled **Enable** or **Disable** button: they are always on, or they are turned on and off automatically with the features that need them.

## Turn several features on or off at once

1. Open **Tools > Features**.
2. Tick the box in front of each feature, or use **Select all ... features grouped under ... category** at the top of a category.
3. Open **Bulk Actions** and choose **Enable** or **Disable**.

## Manage the features of another site

If your site hosts several sites (tenants), an administrator of the main site can manage the features of each one from **Multi-Tenancy > Tenants**, with the tenant's **Features** action. The page then shows **Features for ... tenant**. See [Tenants](../administration/tenants.md).

## Features that IT turns on

Some features don't add any screen: they connect the site to services such as Redis, cloud storage or monitoring, or change how it runs on the server. They usually need settings in a configuration file as well, so your developer or IT team turns them on. They are still listed below, with the page that explains what they do for you.

## Which page covers each feature

Find the feature by the name shown in **Tools > Features**. The categories are the same as on that screen. The technical name is in brackets. The **Technical Manual** has the details for developers in the [module reference](../../reference/README.md).

### Api

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **GraphQL** (`OrchardCore.Apis.GraphQL`) | Lets other applications, such as a mobile app, ask the site for exactly the content they need, and adds the GraphiQL explorer under Tools. | [Developer tools](../settings/developer-tools.md) |
| **OpenApi** (`OrchardCore.OpenApi`) | Describes the site's web APIs in the standard OpenAPI format for developers and their tools, with its settings in Settings > OpenApi. | [Developer tools](../settings/developer-tools.md) |
| **OpenApi ReDoc UI** (`OrchardCore.OpenApi.ReDocUI`) | Shows a read-only reference of the site's web APIs at the /redoc page, for signed-in people with permission. | [Developer tools](../settings/developer-tools.md) |
| **OpenApi Scalar UI** (`OrchardCore.OpenApi.ScalarUI`) | Shows a modern, interactive reference of the site's web APIs at the /scalar/v1 page, for signed-in people with permission. | [Developer tools](../settings/developer-tools.md) |
| **OpenApi Swagger UI** (`OrchardCore.OpenApi.SwaggerUI`) | Shows an interactive explorer at the /swagger page where signed-in people with permission can try each web API call from the browser. | [Developer tools](../settings/developer-tools.md) |

### Communication

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Azure Communication Services Email** (`OrchardCore.Email.Azure`) | Sends the site's emails through Azure Communication Services, Microsoft's email service, instead of an SMTP mail server. | [Email](../settings/email.md) |
| **Azure Communication Services SMS** (`OrchardCore.Sms.Azure`) | Sends the site's text messages through Azure Communication Services, Microsoft's SMS service. | [SMS](../settings/sms.md) |
| **Email** (`OrchardCore.Email`) | Lets the site send email, with the provider settings in Settings > Communication > Email and a test page under Tools > Testing. | [Email](../settings/email.md) |
| **SMS** (`OrchardCore.Sms`) | Lets the site send text messages, with the provider settings in Settings > Communication > SMS and a test page under Tools > Testing. | [SMS](../settings/sms.md) |
| **SMTP Email Provider** (`OrchardCore.Email.Smtp`) | Sends the site's emails through an SMTP mail server, such as your organization's or your email provider's. | [Email](../settings/email.md) |

### Configuration

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Settings** (`OrchardCore.Settings`) | Always on. Holds the site's settings, such as its name and time zone, in Settings > General. | [General settings](../settings/general.md) |

### Content

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Feeds** (`OrchardCore.Feeds`) | Adds RSS feeds to lists, such as a blog, so readers can follow new items in a feed reader. | [RSS feeds](../search/feeds.md) |
| **Flows** (`OrchardCore.Flows`) | Adds the Flow and Bag parts, which let editors build a content item from a stack of widgets or of items placed inside it. | [Flows and bags](../content/flows-and-bags.md) |
| **Forms** (`OrchardCore.Forms`) | Adds form widgets, such as text boxes and buttons, and workflow activities, so you can build forms like a contact form. | [Forms](../forms-and-automation/forms.md) |
| **Layers** (`OrchardCore.Layers`) | Shows widgets in the theme's zones across many pages, using layers with rules such as the page address or the visitor's role. | [Widgets and layers](../design/widgets-and-layers.md) |
| **Shortcode Templates** (`OrchardCore.Shortcodes.Templates`) | Lets you create your own shortcodes in Design > Shortcodes. | [Shortcodes](../design/shortcodes.md) |
| **Widgets** (`OrchardCore.Widgets`) | Lets a content item show widgets, such as a paragraph or an image, in the zones of the theme. | [Widgets and layers](../design/widgets-and-layers.md) |

### Content Management

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Add Content To Deployment Plan** (`OrchardCore.Contents.Deployment.AddToDeploymentPlan`) | Adds an Add to Deployment Plan action to the content items list, to copy chosen items to another site. | [Import and export](../administration/import-and-export.md) |
| **Admin Dashboard** (`OrchardCore.AdminDashboard`) | Turns the admin dashboard into a board of widgets, such as notes or links for your team, that you arrange yourself. | [Admin dashboard](../design/dashboard.md) |
| **Admin Menu** (`OrchardCore.AdminMenu`) | Lets you add your own menus and shortcuts to the admin menu, in Tools > Admin Menus. | [Admin menus](../design/admin-menu.md) |
| **Alias** (`OrchardCore.Alias`) | Gives a content item a fixed name, its alias, that templates and site builders use to find that one item. | [Titles, URLs and the home page](../content/urls-and-homepage.md) |
| **Archive Later** (`OrchardCore.ArchiveLater`) | Lets editors choose a future date and time at which a content item is unpublished automatically. | [Schedule publishing](../content/scheduling.md) |
| **Content Fields** (`OrchardCore.ContentFields`) | Adds the common fields, such as text, number, date, link, yes/no and content picker, that site builders add to content types. | [Fields](../content/fields.md) |
| **Content Fields Indexing (SQL)** (`OrchardCore.ContentFields.Indexing.SQL`) | Copies the values of common fields into database tables, so queries can find and sort content items by them. No screen of its own. | [Fields](../content/fields.md) |
| **Content Fields Indexing (SQL) - User Picker** (`OrchardCore.ContentFields.Indexing.SQL.UserPicker`) | Copies the values of user picker fields into database tables, so queries can find content items by the users chosen. No screen of its own. | [Fields](../content/fields.md) |
| **Content Preview** (`OrchardCore.ContentPreview`) | Adds a Preview button to the content editor, so you can see a draft the way visitors will before you publish it. | [Preview](../content/preview.md) |
| **Content Quick Navigation** (`OrchardCore.Contents.QuickNavigation`) | Adds the most recently updated content items to the admin's quick navigation search, so you can jump straight to them. | [Find your way around](finding-your-way.md) |
| **Content Types** (`OrchardCore.ContentTypes`) | Lets site builders create and change content types, parts and fields in Design > Content Definition. | [Content types](../content/content-types.md) |
| **Content Version Pruning** (`OrchardCore.Contents.VersionPruning`) | Deletes old versions of content items once a day, keeping the ones you choose in Settings > Content Version Pruning. | [Content items](../content/content-items.md) |
| **Contents** (`OrchardCore.Contents`) | Lets people create, edit, publish, unpublish and delete content items in Content > Content Items; most content features need it. | [Content items](../content/content-items.md) |
| **Export Content To Deployment Target** (`OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget`) | Adds an Export to Deployment Target action to the content items list, to send chosen items as a download or to another site. | [Import and export](../administration/import-and-export.md) |
| **File Content Definition** (`OrchardCore.Contents.FileContentDefinition`) | Keeps the content type definitions in a file on the server instead of the database. No screen; your IT team sets it up. | [Content types](../content/content-types.md) |
| **Html** (`OrchardCore.Html`) | Adds the HTML body part, a rich text editor for the main text of a content item. | [Edit text](../content/editing-text.md) |
| **Liquid** (`OrchardCore.Liquid`) | Adds the Liquid part and field, where trusted people write Liquid template code that is shown on a content item's page. | [Templates and placements](../design/templates-and-placements.md) |
| **Liquid Core Services** (`OrchardCore.Liquid.Core`) | Always on, and turned on automatically. Runs the Liquid template language used by templates, permalinks and many settings; no screen of its own. | [Templates and placements](../design/templates-and-placements.md) |
| **Lists** (`OrchardCore.Lists`) | Adds the List part, which turns a content item into a container of other items, such as a blog holding its posts. | [Lists and blogs](../content/lists-and-blogs.md) |
| **Markdown** (`OrchardCore.Markdown`) | Adds the Markdown body part and field, so editors can format text with Markdown, a simple writing syntax. | [Edit text](../content/editing-text.md) |
| **Media** (`OrchardCore.Media`) | Adds the media library, where you upload, organize and pick images and files, and the Media field that attaches them to content. | [Media library](../media/media-library.md) |
| **Media Cache** (`OrchardCore.Media.Cache`) | Keeps local copies of media files stored in the cloud, so the site doesn't fetch them each time; turns on with cloud storage. | [Media settings](../media/media-settings.md) |
| **Media SignalR** (`OrchardCore.Media.SignalR`) | Updates an open media library by itself when someone uploads, renames, moves or deletes a file, without reloading the page. | [Media library](../media/media-library.md) |
| **Media Slugify** (`OrchardCore.Media.Slugify`) | Turns the names of new media files and folders into web-friendly names: lowercase, with dashes and without special characters. | [Media settings](../media/media-settings.md) |
| **Media TUS Uploads** (`OrchardCore.Media.Tus`) | Makes media uploads resumable: you can pause an upload, and an interrupted upload continues where it stopped. | [Media library](../media/media-library.md) |
| **Publish Later** (`OrchardCore.PublishLater`) | Lets editors choose a future date and time at which a content item is published automatically. | [Schedule publishing](../content/scheduling.md) |
| **Queries** (`OrchardCore.Queries`) | Lets you save queries that list content, in Search > Queries, for use in templates, widgets and other applications. | [Queries](../search/queries.md) |
| **Queries Core Services** (`OrchardCore.Queries.Core`) | Turned on automatically when another feature needs it. Provides the query services that queries and search use; no screen of its own. | [Queries](../search/queries.md) |
| **Secure Media** (`OrchardCore.Media.Security`) | Adds view permissions for media folders, so you decide who may open the files of each folder, in the library and on the site. | [Media library](../media/media-library.md) |
| **SEO** (`OrchardCore.Seo`) | Adds the SEO part for page titles, descriptions and social sharing, and the robots.txt settings that guide search engines. | [SEO](../content/seo.md) |
| **Sitemaps** (`OrchardCore.Sitemaps`) | Creates sitemaps that tell search engines which pages your site has, managed in Tools > Search Engine Optimization. | [Sitemaps](../search/sitemaps.md) |
| **Sitemaps Cleanup** (`OrchardCore.Sitemaps.Cleanup`) | Runs a background task every five minutes that deletes saved sitemap files that no longer belong to a sitemap. No screen of its own. | [Sitemaps](../search/sitemaps.md) |
| **Sitemaps for Decoupled Razor Pages** (`OrchardCore.Sitemaps.RazorPages`) | Lets sitemaps include pages your developer builds as Razor Pages; the developer sets it up and it has no screen of its own. | [Sitemaps](../search/sitemaps.md) |
| **Spatial** (`OrchardCore.Spatial`) | Adds the Geo Point field, a place on a map given by its latitude and longitude, such as the address of a shop. | [Fields](../content/fields.md) |
| **SQL Queries** (`OrchardCore.Queries.Sql`) | Lets you write queries in SQL, the database's query language, and test them in Search > Queries > Run SQL Query. | [Queries](../search/queries.md) |
| **Taxonomies** (`OrchardCore.Taxonomies`) | Lets you sort content items into categories and tags, using taxonomies and the Taxonomy field. | [Taxonomies](../content/taxonomies.md) |
| **Taxonomies Contents List Filters** (`OrchardCore.Taxonomies.ContentsAdminList`) | Adds menus to Content > Content Items that filter the list by taxonomy terms, such as a category. | [Taxonomies](../content/taxonomies.md) |
| **Title** (`OrchardCore.Title`) | Gives content items a title, shown in admin lists and on their page. | [Titles, URLs and the home page](../content/urls-and-homepage.md) |
| **View Or Download Content As JSON** (`OrchardCore.Contents.Deployment.Download`) | Adds View as JSON and Download as JSON actions to the content items list, to see or save an item in a technical format. | [Import and export](../administration/import-and-export.md) |

### Data

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Data Pipelines** (`OrchardCore.DataPipelines`) | Lets you design pipelines, without code, that read data, transform it and deliver it as files, emails, download links or content items. | [Data pipelines](../forms-and-automation/data-pipelines.md) |
| **Data Pipelines - FTP** (`OrchardCore.DataPipelines.Ftp`) | Lets data pipelines upload the files they create to FTP servers. | [Data pipelines](../forms-and-automation/data-pipelines.md) |
| **Data Pipelines - SFTP** (`OrchardCore.DataPipelines.Sftp`) | Lets data pipelines upload the files they create to SFTP servers, a secure kind of file server. | [Data pipelines](../forms-and-automation/data-pipelines.md) |
| **Data Pipelines Core** (`OrchardCore.DataPipelines.Core`) | Turned on automatically when another feature needs it. Provides the data sources and file formats pipelines use, and the Tools > Shared Links screen. | [Data pipelines](../forms-and-automation/data-pipelines.md), [Data sources](../forms-and-automation/data-sources.md) |

### Deployment

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Deployment** (`OrchardCore.Deployment`) | Lets you export the site's content and settings to a file and import them into another site, in Tools > Deployments. | [Import and export](../administration/import-and-export.md) |
| **Remote Deployment** (`OrchardCore.Deployment.Remote`) | Sends a deployment plan straight from one site to another, without downloading and uploading a file. | [Remote deployment](../administration/remote-deployment.md) |

### Development

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Admin Templates** (`OrchardCore.AdminTemplates`) | Lets you write templates that change how parts of the admin look, in Design > Admin Templates. | [Templates and placements](../design/templates-and-placements.md) |
| **Placements** (`OrchardCore.Placements`) | Lets you decide where the parts and fields of a content item appear, and in what order, in Design > Placements. | [Templates and placements](../design/templates-and-placements.md) |
| **Placements file storage** (`OrchardCore.Placements.FileStorage`) | Keeps placements in a file on the server instead of the database; the screens stay the same. Your IT team sets it up. | [Templates and placements](../design/templates-and-placements.md) |
| **Templates** (`OrchardCore.Templates`) | Lets you write templates that change how parts of the public site look, in Design > Templates, with a preview. | [Templates and placements](../design/templates-and-placements.md) |

### Distributed

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Distributed Tenants** (`OrchardCore.Tenants.Distributed`) | Keeps tenants in sync when the site runs on several servers. No screen; your IT team sets it up with a shared cache such as Redis. | [Tenants](../administration/tenants.md) |
| **Redis** (`OrchardCore.Redis`) | Connects the site to Redis, a fast shared memory service for sites that run on several servers. Your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |
| **Redis Bus** (`OrchardCore.Redis.Bus`) | Uses Redis to pass messages between the servers of the site, so they stay in step. No screen; your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |
| **Redis Cache** (`OrchardCore.Redis.Cache`) | Uses Redis as a cache shared by all the servers of the site. No screen of its own; your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |
| **Redis Lock** (`OrchardCore.Redis.Lock`) | Uses Redis so the servers of the site don't run the same job at once. No screen; your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |

### GitHub

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **GitHub Authentication** (`OrchardCore.GitHub.Authentication`) | Lets people sign in to the site with their GitHub account. | [External sign-in](../users/external-sign-in.md) |

### Google

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Google Analytics** (`OrchardCore.Google.Analytics`) | Adds Google Analytics to the site's pages, so you can see in Google how many people visit and what they read. | [Analytics and social](../settings/analytics-and-social.md) |
| **Google Authentication** (`OrchardCore.Google.GoogleAuthentication`) | Lets people sign in to the site with their Google account. | [External sign-in](../users/external-sign-in.md) |
| **Google Tag Manager** (`OrchardCore.Google.TagManager`) | Adds Google Tag Manager to the site's pages, so your marketing team can manage tracking tags from Google. | [Analytics and social](../settings/analytics-and-social.md) |

### Hosting

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Amazon Media Image Cache** (`OrchardCore.Media.AmazonS3.ImageCache`) | Stores the resized copies of images in Amazon S3 instead of on the server's disk. Your IT team sets it up. | [Media settings](../media/media-settings.md) |
| **Amazon Media Image Cache (Obsolete)** (`OrchardCore.Media.AmazonS3.ImageSharpImageCache`) | Old name kept for existing sites; replaced by Amazon Media Image Cache, which it turns on. | [Media settings](../media/media-settings.md) |
| **Amazon Media Storage** (`OrchardCore.Media.AmazonS3`) | Stores the media files in Amazon S3 cloud storage instead of on the web server's disk. Your IT team sets it up. | [Media settings](../media/media-settings.md) |
| **Azure Media Image Cache** (`OrchardCore.Media.Azure.ImageCache`) | Stores the resized copies of images in Azure Blob Storage instead of on the server's disk. Your IT team sets it up. | [Media settings](../media/media-settings.md) |
| **Azure Media Image Cache (Obsolete)** (`OrchardCore.Media.Azure.ImageSharpImageCache`) | Old name kept for existing sites; replaced by Azure Media Image Cache, which it turns on. | [Media settings](../media/media-settings.md) |
| **Azure Media Storage** (`OrchardCore.Media.Azure.Storage`) | Stores the media files in Azure Blob Storage, Microsoft's cloud storage, instead of on the web server's disk. Your IT team sets it up. | [Media settings](../media/media-settings.md) |

### Indexing

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Indexing** (`OrchardCore.Indexing`) | Lets you create and manage search indexes in Search > Indexes; the search engine features plug into it. | [Site search and indexes](../search/site-search.md) |
| **Indexing Worker** (`OrchardCore.Indexing.Worker`) | Runs a background task every minute that catches up with content changes the search indexes missed. No screen of its own. | [Site search and indexes](../search/site-search.md) |

### Infrastructure

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Admin** (`OrchardCore.Admin`) | Provides the admin: the dashboard, the admin menu and the screens where you manage the site. | [Find your way around](finding-your-way.md), [General settings](../settings/general.md) |
| **Auto Setup** (`OrchardCore.AutoSetup`) | Sets up a new site or tenant automatically from configuration, without anyone filling in the setup screen. Your IT team sets it up. | [Set up a site](setting-up-a-site.md) |
| **Features** (`OrchardCore.Features`) | Always on. Lets administrators turn features on and off in Tools > Features. | [This page](#turn-a-feature-on) |
| **Health Checks** (`OrchardCore.HealthChecks`) | Gives the site an address that monitoring tools call to check it is up. No screen of its own; your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |
| **Home Route** (`OrchardCore.HomeRoute`) | Remembers which content item is the home page, chosen with Set as homepage in the item's editor. No screen of its own. | [Titles, URLs and the home page](../content/urls-and-homepage.md) |
| **Orchard Diagnostics** (`OrchardCore.Diagnostics`) | Shows visitors a friendly error page in the site's theme, such as Not Found, instead of a technical error. No settings. | [General settings](../settings/general.md) |
| **Recipes** (`OrchardCore.Recipes`) | Always on. Lets administrators run recipes, ready-made sets of content and settings, in Tools > Recipes. | [Recipes](../administration/recipes.md) |
| **Recipes Core Services** (`OrchardCore.Recipes.Core`) | Turned on automatically when another feature needs it. Runs recipes during setup, import and deployment; no screen of its own. | [Recipes](../administration/recipes.md) |
| **Remote Publishing** (`OrchardCore.RemotePublishing`) | Lets people write and publish blog posts from a desktop blogging app, such as Open Live Writer, instead of the admin. | [Developer tools](../settings/developer-tools.md) |
| **Response Compression** (`OrchardCore.ResponseCompression`) | Compresses pages before sending them, so they download faster. No settings; it works for every page while it is on. | [Security](../settings/security.md) |
| **Reverse Proxy Configuration** (`OrchardCore.ReverseProxy`) | Lets the site trust the visitor details passed on by a proxy or load balancer in front of it, in Settings > Reverse Proxy. | [Security](../settings/security.md) |
| **Rules** (`OrchardCore.Rules`) | Provides the conditions, such as the page address, the visitor's role or the culture, that decide when a layer shows its widgets. | [Widgets and layers](../design/widgets-and-layers.md) |
| **Scripting** (`OrchardCore.Scripting`) | Always on. Runs the small scripts used in workflows, layer rules and recipes; no screen of its own. | [Workflows](../forms-and-automation/workflows.md) |
| **Setup** (`OrchardCore.Setup`) | Shows the setup screen of a new site or tenant, where you choose a recipe and create the first account; off after setup. | [Set up a site](setting-up-a-site.md) |
| **Shortcodes** (`OrchardCore.Shortcodes`) | Lets editors type short codes in square brackets, such as [image], that the site replaces with richer content when the page is shown. | [Shortcodes](../design/shortcodes.md) |
| **SignalR** (`OrchardCore.SignalR`) | Lets the site send live updates to open pages, such as a refreshed media library. No screen; your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |
| **SignalR Azure Backplane** (`OrchardCore.SignalR.Azure`) | Makes live updates work when the site runs on several servers, using the Azure SignalR Service. Your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |
| **SignalR Redis Backplane** (`OrchardCore.SignalR.Redis`) | Makes live updates work when the site runs on several servers, using Redis. Your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |
| **Static File Provider** (`OrchardCore.Tenants.FileProvider`) | Gives each tenant its own server folder for plain files, such as robots.txt. No screen; your IT team puts the files there. | [Tenants](../administration/tenants.md) |
| **Tenant Feature Profiles** (`OrchardCore.Tenants.FeatureProfiles`) | Lets you limit which features each tenant may use, with feature profiles in Multi-Tenancy > Feature Profiles. | [Tenants](../administration/tenants.md) |
| **Tenants** (`OrchardCore.Tenants`) | Lets you run several separate sites, called tenants, from one installation, and manage them in Multi-Tenancy > Tenants. | [Tenants](../administration/tenants.md) |
| **URL Rewriting** (`OrchardCore.UrlRewriting`) | Lets you redirect old addresses to new ones, or show a page under another address, with rules in Tools > URL Rewriting. | [URL rewriting](../settings/url-rewriting.md) |
| **XML-RPC** (`OrchardCore.XmlRpc`) | Provides the connection that desktop blogging apps use to publish to the site; Remote Publishing needs it. No screen of its own. | [Developer tools](../settings/developer-tools.md) |

### Internationalization

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Admin Culture Picker** (`OrchardCore.Localization.AdminCulturePicker`) | Adds a language menu to the admin's top bar, so each editor can choose the language of the admin screens. | [Translate content](../content/localization.md) |
| **Content Culture Picker** (`OrchardCore.ContentLocalization.ContentCulturePicker`) | Adds a language menu to the public site, so visitors can switch to the translation of the page they are on. | [Translate content](../content/localization.md) |
| **Content Language Header** (`OrchardCore.Localization.ContentLanguageHeader`) | Tells browsers and search engines the language of every page the site sends. No screen of its own. | [Translate content](../content/localization.md) |
| **Content Localization** (`OrchardCore.ContentLocalization`) | Lets editors translate content items into the site's other languages and link the translations together. | [Translate content](../content/localization.md) |
| **Data Localization** (`OrchardCore.DataLocalization`) | Lets you translate names people typed into the site, such as content type and field names, in Settings > Localization > Dynamic Translations. | [Translate content](../content/localization.md) |
| **Localization** (`OrchardCore.Localization`) | Lets you choose the languages (cultures) of the site in Settings > Localization > Cultures, and shows the admin translated into them. | [Translate content](../content/localization.md) |
| **Localized Content Item Sitemaps** (`OrchardCore.ContentLocalization.Sitemaps`) | Makes sitemaps link each page to its translations, so search engines know they are the same page in other languages. | [Sitemaps](../search/sitemaps.md) |

### Meta

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Meta Core Components** (`OrchardCore.Facebook`) | Turned on with the other Meta features. Connects the site to your Meta app, set up in Settings > Integrations > Meta App. | [Analytics and social](../settings/analytics-and-social.md) |
| **Meta Login** (`OrchardCore.Facebook.Login`) | Lets people sign in to the site with their Facebook account. | [External sign-in](../users/external-sign-in.md) |
| **Meta Pixel** (`OrchardCore.Facebook.Pixel`) | Adds the Meta Pixel to the site's pages, so Meta can tell which visitors came from your ads and what they did. | [Analytics and social](../settings/analytics-and-social.md) |
| **Meta Social Plugins Widgets** (`OrchardCore.Facebook.Widgets`) | Adds widgets for Meta's social plugins, such as Like buttons and comments, that you place on pages. | [Analytics and social](../settings/analytics-and-social.md) |

### Microsoft Authentication

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Microsoft Account Authentication** (`OrchardCore.Microsoft.Authentication.MicrosoftAccount`) | Lets people sign in to the site with their personal Microsoft account. | [External sign-in](../users/external-sign-in.md) |
| **Microsoft Entra ID (Azure Active Directory) Authentication** (`OrchardCore.Microsoft.Authentication.AzureAD`) | Lets people sign in to the site with their work or school account from Microsoft Entra ID. | [External sign-in](../users/external-sign-in.md) |

### Navigation

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Autoroute** (`OrchardCore.Autoroute`) | Gives each content item its own web address (permalink), built automatically from a pattern such as its title. | [Titles, URLs and the home page](../content/urls-and-homepage.md) |
| **Menu** (`OrchardCore.Menu`) | Lets you build the site's navigation menus as content items, in Content > Menus. | [Menus](../design/menus.md) |
| **Navigation** (`OrchardCore.Navigation`) | Always on. Shows menus and page numbers (paging) on the site and in the admin; no screen of its own. | [Menus](../design/menus.md) |

### Notifications

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Email Notifications** (`OrchardCore.Notifications.Email`) | Lets people also receive their notifications by email. | [Notifications](../forms-and-automation/notifications.md) |
| **Notifications** (`OrchardCore.Notifications`) | Adds the notification center, a bell in the admin's top bar where each person reads the messages the site sends them. | [Notifications](../forms-and-automation/notifications.md) |
| **SMS Notifications** (`OrchardCore.Notifications.Sms`) | Lets people also receive their notifications by text message. | [Notifications](../forms-and-automation/notifications.md) |

### OpenID Connect

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **OpenID Connect Authorization Server** (`OrchardCore.OpenId.Server`) | Makes the site a sign-in service for other applications, which then let people sign in with their account on this site. | [OpenID Connect server](../users/openid-server.md) |
| **OpenID Connect Client Integration** (`OrchardCore.OpenId.Client`) | Lets people sign in to the site through your organization's own identity provider, or any other OpenID Connect service. | [External sign-in](../users/external-sign-in.md) |
| **OpenID Connect Core Services** (`OrchardCore.OpenId`) | Turned on automatically when another OpenID Connect feature needs it. Provides their shared services; no screen of its own. | [OpenID Connect server](../users/openid-server.md) |
| **OpenID Connect Management UI** (`OrchardCore.OpenId.Management`) | Adds the screens to manage the applications and scopes allowed to use the site's sign-in service, under Access Control > OpenID Connect. | [OpenID Connect server](../users/openid-server.md) |
| **OpenID Connect Secrets** (`OrchardCore.OpenId.Secrets`) | Lets the site's sign-in service use signing and encryption keys kept in the site's secrets. | [OpenID Connect server](../users/openid-server.md), [Secrets](../settings/secrets.md) |
| **OpenID Connect Token Validation** (`OrchardCore.OpenId.Validation`) | Lets the site accept access tokens from its own sign-in service or another trusted one, set in Settings > OpenID Connect > Token Validation. | [OpenID Connect server](../users/openid-server.md) |

### Performance

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Dynamic Cache** (`OrchardCore.DynamicCache`) | Keeps ready-made copies of parts of your pages, such as menus and widgets, so the site doesn't rebuild them on every visit. | [General settings](../settings/general.md) |
| **Mini Profiler** (`OrchardCore.MiniProfiler`) | Shows people with permission a small box on each page with the time it took to build, to find slow pages. | [Developer tools](../settings/developer-tools.md) |

### Search

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Azure AI Search** (`OrchardCore.AzureAI`) | Lets the site keep search indexes in Azure AI Search, Microsoft's cloud search service, to power site search. | [Site search and indexes](../search/site-search.md) |
| **Azure AI Search (Obsolete)** (`OrchardCore.Search.AzureAI`) | Old name kept for existing sites; replaced by Azure AI Search, which it turns on. | [Site search and indexes](../search/site-search.md) |
| **Elasticsearch** (`OrchardCore.Elasticsearch`) | Lets the site keep search indexes on an Elasticsearch server, to power site search and queries. Your IT team sets up the server. | [Site search and indexes](../search/site-search.md), [Queries](../search/queries.md) |
| **Elasticsearch (Obsolete)** (`OrchardCore.Search.Elasticsearch`) | Old name kept for existing sites; replaced by Elasticsearch, which it turns on. | [Site search and indexes](../search/site-search.md) |
| **Elasticsearch Content Picker** (`OrchardCore.Search.Elasticsearch.ContentPicker`) | Adds an editor to the Content Picker field that finds items through an Elasticsearch index, handy when there are many items. | [Site search and indexes](../search/site-search.md), [Fields](../content/fields.md) |
| **Elasticsearch Worker** (`OrchardCore.Search.Elasticsearch.Worker`) | Kept for older sites; replaced by Indexing Worker, which keeps the search indexes up to date. | [Site search and indexes](../search/site-search.md) |
| **Lucene** (`OrchardCore.Lucene`) | Lets the site keep search indexes in files on the web server, to power site search and queries, with nothing else to install. | [Site search and indexes](../search/site-search.md), [Queries](../search/queries.md) |
| **Lucene (Obsolete)** (`OrchardCore.Search.Lucene`) | Old name kept for existing sites; replaced by Lucene, which it turns on. | [Site search and indexes](../search/site-search.md) |
| **Lucene Content Picker** (`OrchardCore.Search.Lucene.ContentPicker`) | Adds an editor to the Content Picker field that finds items through a Lucene index, handy when there are many items. | [Site search and indexes](../search/site-search.md), [Fields](../content/fields.md) |
| **Lucene Worker** (`OrchardCore.Search.Lucene.Worker`) | Kept for older sites; replaced by Indexing Worker, which keeps the search indexes up to date. | [Site search and indexes](../search/site-search.md) |
| **Media Indexing** (`OrchardCore.Media.Indexing`) | Turned on automatically by the media indexing features. Lets search indexes include the text of documents attached to content items. | [Site search and indexes](../search/site-search.md) |
| **OpenXML Media Indexing** (`OrchardCore.Media.Indexing.OpenXML`) | Lets site search find words inside Word (.docx) and PowerPoint (.pptx) files attached to content items. | [Site search and indexes](../search/site-search.md) |
| **PDF Media Indexing** (`OrchardCore.Media.Indexing.Pdf`) | Lets site search find words inside PDF files attached to content items. | [Site search and indexes](../search/site-search.md) |
| **Search** (`OrchardCore.Search`) | Adds the search page and the Search Form widget, so visitors can search your content, with settings in Settings > Search. | [Site search and indexes](../search/site-search.md) |
| **Text Media Indexing** (`OrchardCore.Media.Indexing.Text`) | Lets site search find words inside text (.txt) and Markdown (.md) files attached to content items. | [Site search and indexes](../search/site-search.md) |

### Security

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Audit Trail** (`OrchardCore.AuditTrail`) | Records who created, changed, published or deleted content and when, and lets you compare and restore earlier versions. | [Audit trail](../content/audit-trail.md) |
| **ClamAV Antivirus Scanner** (`OrchardCore.Antivirus.ClamAV`) | Checks uploaded media files and imported packages with the ClamAV virus scanner before the site stores them. Your IT team sets it up. | [Media settings](../media/media-settings.md) |
| **CORS Configuration** (`OrchardCore.Cors`) | Lets you decide which other websites may call your site from their pages, in Settings > Security > Cross-Origin Resource Sharing. | [Security](../settings/security.md) |
| **Distributed Data Protection (Azure Storage)** (`OrchardCore.DataProtection.Azure`) | Keeps the keys that protect sign-ins and encrypted settings in Azure Blob Storage, shared by all servers. Your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |
| **Distributed Data Protection (Redis)** (`OrchardCore.Redis.DataProtection`) | Keeps the keys that protect sign-ins and encrypted settings in Redis, shared by all servers. Your IT team sets it up. | [Developer tools](../settings/developer-tools.md) |
| **External Authentication** (`OrchardCore.Users.ExternalAuthentication`) | Turned on with the external sign-in features. Holds the shared options for signing in with an account from another service. | [External sign-in](../users/external-sign-in.md) |
| **HTTPS** (`OrchardCore.Https`) | Makes visitors use a secure https:// connection, with the options in Settings > Security > HTTPS. | [Security](../settings/security.md) |
| **Rate Limits** (`OrchardCore.RateLimits`) | Slows down or blocks visitors who send too many requests, with policies in Tools > Rate Limits. | [Security](../settings/security.md) |
| **ReCaptcha** (`OrchardCore.ReCaptcha`) | Connects the site to Google reCAPTCHA, which asks visitors to prove they are not robots, set in Settings > Security > reCaptcha. | [Security](../settings/security.md) |
| **ReCaptcha Users** (`OrchardCore.ReCaptcha.Users`) | Adds the reCAPTCHA check to the login, registration, forgot password and reset password pages. | [Login and registration](../users/login-and-registration.md) |
| **Roles** (`OrchardCore.Roles`) | Lets you create roles, choose their permissions in Access Control > Roles, and give roles to users. | [Roles](../users/roles.md) |
| **Roles Core Services** (`OrchardCore.Roles.Core`) | Turned on automatically when another feature needs it. Provides the role services other features use; no screen of its own. | [Roles](../users/roles.md) |
| **Secrets** (`OrchardCore.Secrets`) | Stores passwords, API keys and certificates once, encrypted, so settings screens can use them by name, in Tools > Security > Secrets. | [Secrets](../settings/secrets.md) |
| **Secrets - Azure Key Vault** (`OrchardCore.Secrets.AzureKeyVault`) | Lets secrets be kept in Azure Key Vault, Microsoft's secure vault service. Your IT team connects the vault. | [Secrets](../settings/secrets.md) |
| **Security** (`OrchardCore.Security`) | Sends security instructions to browsers with every page, set in Settings > Security > Security Headers. | [Security](../settings/security.md) |
| **Two-Factor Authentication Services** (`OrchardCore.Users.2FA`) | Turned on with a two-factor method. Adds two-factor sign-in, where people confirm their identity with a code after their password. | [Login and registration](../users/login-and-registration.md), [Sign in and your profile](signing-in.md) |
| **Two-Factor Authenticator App Method** (`OrchardCore.Users.2FA.AuthenticatorApp`) | Lets people confirm their sign-in with a code from an authenticator app on their phone. | [Login and registration](../users/login-and-registration.md), [Sign in and your profile](signing-in.md) |
| **Two-Factor Email Method** (`OrchardCore.Users.2FA.Email`) | Lets people confirm their sign-in with a code sent to them by email. | [Login and registration](../users/login-and-registration.md), [Sign in and your profile](signing-in.md) |
| **Two-Factor SMS Method** (`OrchardCore.Users.2FA.Sms`) | Lets people confirm their sign-in with a code sent to them by text message. | [Login and registration](../users/login-and-registration.md), [Sign in and your profile](signing-in.md) |
| **Users** (`OrchardCore.Users`) | Adds user accounts, the login page and the screens to manage users in Access Control > Users. | [Users](../users/users.md) |
| **Users Audit Trail** (`OrchardCore.Users.AuditTrail`) | Records user events, such as sign-ins, failed sign-ins and account changes, in the audit trail. | [Users](../users/users.md) |
| **Users Authentication Ticket Store** (`OrchardCore.Users.Authentication.CacheTicketStore`) | Keeps people's sign-in details on the server instead of in a large browser cookie. No screen of its own. | [Users](../users/users.md) |
| **Users Change Email** (`OrchardCore.Users.ChangeEmail`) | Lets people change the email address of their account themselves, when the administrator allows it. | [Login and registration](../users/login-and-registration.md), [Sign in and your profile](signing-in.md) |
| **Users Registration** (`OrchardCore.Users.Registration`) | Lets visitors create their own account on the site, optionally confirming their email address. | [Login and registration](../users/login-and-registration.md) |
| **Users Reset Password** (`OrchardCore.Users.ResetPassword`) | Lets people who forgot their password reset it themselves through an emailed link, when the administrator allows it. | [Login and registration](../users/login-and-registration.md), [Sign in and your profile](signing-in.md) |

### Settings

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Custom Settings** (`OrchardCore.CustomSettings`) | Lets site builders turn a content type into a page of site settings, shown under the Settings menu. | [Custom settings](../settings/custom-settings.md) |
| **Custom User Settings** (`OrchardCore.Users.CustomUserSettings`) | Lets site builders turn a content type into extra settings for each user, filled in on the user's account and profile. | [Custom user settings](../users/custom-user-settings.md) |
| **User Localization** (`OrchardCore.Users.Localization`) | Lets each user have their own admin language, chosen in the user editor and in their profile. | [Users](../users/users.md) |
| **User Time Zone** (`OrchardCore.Users.TimeZone`) | Lets each user have their own time zone, used to show and edit dates in the admin. | [Users](../users/users.md) |

### Tasks

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Background Tasks** (`OrchardCore.BackgroundTasks`) | Lets you see the site's scheduled background jobs, run them, turn them off and change their schedule, in Tools > Background Tasks. | [Background tasks](../settings/background-tasks.md) |

### Theming

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Resources** (`OrchardCore.Resources`) | Always on. Loads the site's scripts and stylesheets, with options in the Resources tab of Settings > General. | [General settings](../settings/general.md) |
| **Themes** (`OrchardCore.Themes`) | Lets you choose the theme of the public site and of the admin, in Design > Themes. | [Themes](../design/themes.md) |

### Workflows

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **HTTP Workflows Activities** (`OrchardCore.Workflows.Http`) | Adds workflow activities that start a workflow when an address is called, call other web services and redirect visitors. | [Workflows](../forms-and-automation/workflows.md) |
| **Session Workflows Activities** (`OrchardCore.Workflows.Session`) | Adds the Commit Transaction Task, which saves a workflow's changes to the database right away instead of at the end. | [Workflows](../forms-and-automation/workflows.md) |
| **Timer Workflows Activities** (`OrchardCore.Workflows.Timers`) | Adds the Timer Event, which starts a workflow on a schedule, such as every Monday morning. | [Workflows](../forms-and-automation/workflows.md) |
| **Workflows** (`OrchardCore.Workflows`) | Lets you automate tasks without code, such as sending an email when a form is sent, by drawing workflows in Design > Workflows. | [Workflows](../forms-and-automation/workflows.md) |

### X (Twitter)

| Feature | What it does | User Manual page |
| --- | --- | --- |
| **Sign in with X (Twitter)** (`OrchardCore.Twitter.Signin`) | Lets people sign in to the site with their X (Twitter) account. | [External sign-in](../users/external-sign-in.md) |
| **X (Twitter) Integration** (`OrchardCore.Twitter`) | Lets workflows post on your X (Twitter) account, for example when an article is published. | [Analytics and social](../settings/analytics-and-social.md) |
