---
title: Glossary
description: Plain-language definitions of the words used in the Orchard Core admin and in this manual, from activity to zone, each with a link to the page that explains it.
technical_manual:
  - reference/glossary/README.md
---

# Glossary

The words you meet in the admin and in this manual, in plain language. Each term links to the page that explains it.

[A](#a) · [B](#b) · [C](#c) · [D](#d) · [E](#e) · [F](#f) · [H](#h) · [I](#i) · [L](#l) · [M](#m) · [N](#n) · [O](#o) · [P](#p) · [Q](#q) · [R](#r) · [S](#s) · [T](#t) · [U](#u) · [V](#v) · [W](#w) · [Z](#z)

## A

- **Activity**: one box in a workflow. An activity is either an **event**, which starts or resumes the workflow when something happens (a form is posted, content is published), or a **task**, which does something (send an email, publish an item). See [Workflows](forms-and-automation/workflows.md).
- **Admin**: the back office of the site, under `/Admin`, where you manage content, design and settings. Visitors never see it. See [Finding your way](getting-started/finding-your-way.md).
- **Admin menu**: the menu on the left of the admin. Its entries depend on the features that are on and on your permissions. Administrators can also add custom admin menus in **Tools > Admin Menus**. See [Admin menu](design/admin-menu.md).
- **Admin template**: a template that changes how a piece of the admin looks, written in Liquid in **Design > Admin Templates**. See [Templates and placements](design/templates-and-placements.md).
- **Alias**: a short, fixed name for a content item, such as `main-menu`, that templates and themes use to find it. It isn't an address. See [URLs and the home page](content/urls-and-homepage.md).
- **Anonymous**: the built-in role that stands for visitors who aren't signed in. Its permissions decide what the public can see and do. See [Roles](users/roles.md).
- **Audit trail**: a history of who changed what, and when, with the option to compare and restore versions. See [Audit trail](content/audit-trail.md).
- **Authenticated**: the built-in role that every signed-in user has, whatever their other roles. See [Roles](users/roles.md).
- **Autoroute**: the part that gives a content item its own web address (its **permalink**), usually made from its title. See [URLs and the home page](content/urls-and-homepage.md).

## B

- **Background task**: work the site does on its own on a schedule, such as publishing scheduled items or cleaning caches. See [Background tasks](settings/background-tasks.md).
- **Bag**: a part that holds a set of content items of chosen types inside another item, such as slides in a slideshow. See [Flows and bags](content/flows-and-bags.md).
- **Body**: the main text of a content item, given by an **HtmlBody**, **MarkdownBody** or **Liquid** part. See [Editing text](content/editing-text.md).

## C

- **Clone**: make a copy of a content item, as a new draft. See [Content items](content/content-items.md).
- **Condition**: one test in a **rule**, such as "the page address starts with `/shop`" or "the visitor is signed in". See [Widgets and layers](design/widgets-and-layers.md).
- **Container**: a content item that holds other content items through the **List** part, such as a blog holding its posts. See [Lists and blogs](content/lists-and-blogs.md).
- **Content item**: one piece of content: a page, a blog post, a menu, a widget. Every content item has a content type. See [Content items](content/content-items.md).
- **Content type**: the blueprint of a kind of content item, such as **Page** or **Blog Post**: which parts and fields it has and how it behaves. See [Content types](content/content-types.md).
- **Culture**: a language, often with a country, written like `en-US` or `fr-FR`. A multilingual site supports several cultures, one of them the default. See [Localization](content/localization.md).
- **Culture picker**: the menu that lets visitors switch the site's language. See [Localization](content/localization.md).
- **Custom settings**: site settings that you design yourself, from a content type with the **Custom Settings** stereotype. See [Custom settings](settings/custom-settings.md).

## D

- **Dashboard**: the first page of the admin, which can show dashboard widgets. See [Dashboard](design/dashboard.md).
- **Data pipeline**: a series of steps that reads data, transforms it and delivers it somewhere, such as a file or an email. See [Data pipelines](forms-and-automation/data-pipelines.md).
- **Data source**: a place data pipelines read from, such as content items, users or a query. See [Data sources](forms-and-automation/data-sources.md).
- **Deployment plan**: a saved list of things to export from a site (content, media, settings), which you run to get a file or to send them to another site. See [Import and export](administration/import-and-export.md).
- **Display text**: the name the admin shows for a content item, usually its title.
- **Draft**: a saved version of a content item that visitors don't see yet. A published item can have a draft with changes waiting to be published. See [Content items](content/content-items.md).
- **Draftable**: a content type setting. Only draftable types have a **Save Draft** button. See [Content types](content/content-types.md).
- **Dynamic translations**: translations of texts typed in the admin, such as content type names, made in **Settings > Localization > Dynamic Translations**. See [Localization](content/localization.md).

## E

- **Event**: an activity that starts or resumes a workflow when something happens. See *Activity*.
- **Export**: copy things out of a site into a file, or to another site, with a deployment plan. See [Import and export](administration/import-and-export.md).

## F

- **Feature**: a piece of Orchard Core that an administrator turns on or off in **Tools > Features**. Its menus and screens appear only while it's on. See [Features](getting-started/features.md).
- **Feature profile**: a list of the features a tenant may use. See [Tenants](administration/tenants.md).
- **Feed**: an RSS address that news readers follow to learn about new items, such as blog posts. See [RSS feeds](search/feeds.md).
- **Field**: one piece of information on a content item, such as a date, an image, a link or a tag list. Fields are added to content types. See [Fields](content/fields.md).
- **Flow**: a part that lets editors build the body of a content item from widgets, stacked and laid out in columns. See [Flows and bags](content/flows-and-bags.md).
- **Form**: a set of form widgets (text boxes, lists, a button) that visitors fill in and send, usually handled by a workflow. See [Forms](forms-and-automation/forms.md).

## H

- **Home page**: the content item shown at the site's root address. You choose it with **Set as homepage** on an item. See [URLs and the home page](content/urls-and-homepage.md).

## I

- **Import**: bring content, settings or other things into a site from a file made by an export or a recipe. See [Import and export](administration/import-and-export.md).
- **Index**: a copy of your content organized for fast searching, kept up to date as content changes. Site search and some queries use indexes. See [Site search](search/site-search.md).
- **Instance**: one run of a workflow, with its history. See [Workflows](forms-and-automation/workflows.md).

## L

- **Layer**: a named set of widgets that appear together wherever the layer's **rule** is true, such as **Always** (every page) or **Homepage**. See [Widgets and layers](design/widgets-and-layers.md).
- **Liquid**: a simple template language used in many admin fields to insert values, such as `{{ Site.SiteName }}`. See [Templates and placements](design/templates-and-placements.md).
- **List**: the part that turns a content item into a container of other items. See [Lists and blogs](content/lists-and-blogs.md).
- **Localization**: making content and the site available in several cultures. See [Localization](content/localization.md).

## M

- **Media library**: the place where the site's images, documents and other files are stored, in folders. See [Media library](media/media-library.md).
- **Media profile**: a named set of image sizes and crops, such as *banner*, that templates use to resize images. See [Media settings](media/media-settings.md).
- **Menu**: a content item that holds the links of a site navigation menu. See [Menus](design/menus.md).
- **Module**: a package of code that brings one or more features. Developers install modules; you turn their features on. See [Features](getting-started/features.md).

## N

- **Notification**: a message to a user, shown in the bell icon of the admin's top bar and the **Notification Center**, and optionally sent by email or SMS. See [Notifications](forms-and-automation/notifications.md).

## O

- **Outcome**: one possible result of a workflow activity, such as **Done**, **Valid** or **Failed**. You connect each outcome to the activity that should run next. See [Workflows](forms-and-automation/workflows.md).
- **Owner**: the user a content item belongs to, usually the person who created it. Permissions such as *Edit own content* apply to the items you own. See [Content items](content/content-items.md).

## P

- **Part**: a reusable piece of behavior added to a content type, such as **Title**, **Autoroute**, **HtmlBody**, **List** or **Flow**. A part can hold several fields. See [Content types](content/content-types.md).
- **Permalink**: the permanent web address of a content item, such as `/about`. See [URLs and the home page](content/urls-and-homepage.md).
- **Permission**: the right to do one thing, such as *Publish or unpublish content for others*. Roles hold permissions; users get them through their roles. See [Roles and permissions](getting-started/roles-and-permissions.md).
- **Placement**: rules that decide where each part and field of a content item shows, and in what order, in its page or editor. See [Templates and placements](design/templates-and-placements.md).
- **Preview**: see a content item as visitors will, including unsaved changes, before you publish it. See [Content preview](content/preview.md).
- **Publish**: make a content item visible on the site. **Unpublish** takes it off the site again, without deleting it. See [Content items](content/content-items.md).
- **Publish Later**: schedule a content item to publish itself at a date and time. See [Scheduling](content/scheduling.md).

## Q

- **Query**: a saved search, written in SQL, Lucene or Elasticsearch syntax, that templates and other features use to list content. See [Queries](search/queries.md).

## R

- **Recipe**: a file that sets up a site or adds things to it: features, content types, content, settings. Setup recipes, such as **Blog**, are chosen when a site is created; others run from **Tools > Recipes**. See [Recipes](administration/recipes.md) and [Setting up a site](getting-started/setting-up-a-site.md).
- **Remote instance**, **remote client**: the two ends of a direct link between two sites for sending deployment plans. See [Remote deployment](administration/remote-deployment.md).
- **Role**: a named set of permissions, such as **Editor** or **Contributor**, given to users. See [Roles](users/roles.md).
- **Rule**: the conditions that decide when a layer's widgets show. See [Widgets and layers](design/widgets-and-layers.md).

## S

- **Secret**: a password, key or certificate stored securely and picked by name in settings, instead of being typed into each screen. See [Secrets](settings/secrets.md).
- **Securable**: a content type setting that gives the type its own permissions, such as *Edit Page*. See [Content types](content/content-types.md).
- **Setup**: the screen that creates a new site or tenant: its name, recipe, database and first administrator. See [Setting up a site](getting-started/setting-up-a-site.md).
- **Shape**: a named piece of a page that Orchard Core draws with a template, such as a content item's summary, a menu or a widget. Templates and placements refer to shapes by name. See [Templates and placements](design/templates-and-placements.md).
- **Shortcode**: a short code in square brackets, such as `[image]photo.jpg[/image]`, that you type in text and that the site replaces with something richer. See [Shortcodes](design/shortcodes.md).
- **Site**: the public website that visitors see, as opposed to the admin.
- **Sitemap**: a file that lists your pages for search engines. See [Sitemaps](search/sitemaps.md).
- **Stereotype**: a label on a content type that says what it's used for, such as **Widget**, **Menu Item** or **Custom Settings**, so the admin offers it in the right places. See [Content types](content/content-types.md).

## T

- **Task**: an activity that does something in a workflow, such as sending an email. See *Activity*.
- **Taxonomy**: a list of terms used to classify content, such as *Categories* or *Tags*. See [Taxonomies](content/taxonomies.md).
- **Template**: a piece of Liquid that changes how a shape looks on the site, written in **Design > Templates**. See [Templates and placements](design/templates-and-placements.md).
- **Tenant**: a separate site running in the same Orchard Core installation, with its own content, users, settings and address. See [Tenants](administration/tenants.md).
- **Term**: one entry in a taxonomy, such as *Travel* in *Categories*. See [Taxonomies](content/taxonomies.md).
- **Theme**: the design of the site (the **site theme**) or of the admin (the **admin theme**). See [Themes](design/themes.md).

## U

- **Unpublish**: take a published content item off the site, keeping it in the admin. See [Content items](content/content-items.md).
- **User**: an account that someone signs in with. See [Users](users/users.md).

## V

- **Version**: one saved state of a content item. Versionable types keep earlier versions. See [Audit trail](content/audit-trail.md).

## W

- **Widget**: a block of content, such as a paragraph, an image or a form, placed in a flow or in a zone of the site. Widgets are content items whose type has the **Widget** stereotype. See [Widgets and layers](design/widgets-and-layers.md).
- **Workflow**: an automated process drawn as connected activities, such as "when a form is posted, send an email and redirect". See [Workflows](forms-and-automation/workflows.md).

## Z

- **Zone**: a named area of the site's layout, such as **Content** or **Footer**, where widgets can be placed. The theme decides which zones exist and where they show. See [Widgets and layers](design/widgets-and-layers.md).
