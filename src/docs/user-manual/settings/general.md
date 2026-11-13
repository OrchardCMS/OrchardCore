---
title: General settings
description: Set the site name, time zone, page size, base URL, script and cache options, the look of the admin, and the debugging options.
technical_manual:
  - reference/modules/Settings/README.md
  - reference/modules/Admin/README.md
  - reference/modules/Resources/README.md
  - reference/modules/DynamicCache/README.md
  - reference/modules/Diagnostics/README.md
---

# General settings

The **Settings > General** screen holds the basic facts about your site: its name, its time zone, how many items a list shows per page, and how scripts and caches behave. Two related screens sit next to it: **Settings > Admin** changes how the admin looks and behaves for everyone, and **Settings > Debugging** helps a designer find which template drew a part of a page.

| | |
| --- | --- |
| **Menu** | **Settings > General**, **Settings > Admin**, **Settings > Debugging** |
| **Permission** | *Manage general settings* (General), *Manage Admin Settings* (Admin), *Manage debugging settings* (Debugging). The Administrator role has all three by default. |
| **Feature** | **Settings** (`OrchardCore.Settings`, always on) and **Admin** (`OrchardCore.Admin`) |


## How settings changes apply

Every settings screen has a **Save** button at the bottom. After a successful save you see *Site settings updated successfully.*

Some screens show a yellow warning at the top: *The website might be restarted upon saving the settings, potentially leading to temporary unresponsiveness during the process.* On these screens, saving reloads the site so the new values take effect. The site may answer slowly for a few seconds; nothing is lost. **Settings > General** is one of these screens. Others are listed on their own pages, for example [Security](security.md) and [Analytics and social](analytics-and-social.md).

!!! note
    Some settings can also be fixed by your developer or IT team in the site configuration. When that is the case, the screen may tell you that the current values come from configuration, and what you save there may not change what the site uses.

## Change the site name, time zone and page size

1. Open **Settings > General**. The screen has three tabs: **Site**, **Resources** and **Cache**.
2. On the **Site** tab, change the fields you need.
3. Click **Save**.

| Field | What it does |
| --- | --- |
| **Site name** | The name of your site. Themes often show it in the header and in the browser tab. |
| **Page title format** | Optional. A Liquid template that builds the title shown in the browser tab for every page. Leave it empty to use the page's own title as it is. Ask your designer before changing it. |
| **Base url** | Optional. The full address of the site, such as `https://www.example.com`. It must start with `http://` or `https://` and can't contain a `?` query or a `#` part, otherwise you see *The Base url must be a fully qualified URL.* Features that build links outside a web request, such as emails, use it. |
| **Default Time Zone** | The time zone used to show and edit dates and times in the admin and on the site. **Local to server** uses the time zone of the computer that runs the site. Daylight saving time is applied automatically. [Background tasks](background-tasks.md) also follow this time zone. |
| **Page size** | Required. How many items a list shows per page, for example the content items list. It must be greater than zero, and can't be larger than the maximum page size your IT team set (the hint under **Page size options** shows it, for example *Values cannot exceed the maximum page size of 100.*). |
| **Page size selection** | Check **Allow users to change the page size on listing pages** to show a selector on lists so each person can pick how many items they see. |
| **Page size options** | Shown when page size selection is allowed, and then required. The choices of the selector, separated by commas, for example `10, 25, 50, 100`. Each value must be a positive number no larger than the maximum page size. |

!!! warning "Base url"
    Type the address people really use to reach the site. A wrong base URL breaks the links in the emails and messages the site sends.

## Set the options for scripts and stylesheets

The **Resources** tab controls how the site loads its scripts and stylesheets (the files that make pages look and behave the way they do). Leave the defaults unless your developer asks you to change them.

1. Open **Settings > General** and click the **Resources** tab.
2. Change the options.
3. Click **Save**.

| Field | What it does |
| --- | --- |
| **Use resources cache busting** | On by default. Adds a version to the address of each script and stylesheet, so browsers download a fresh copy when the file changes instead of using an old saved copy. |
| **Use framework CDN (Content Delivery Network)** | Loads common libraries, such as jQuery, from a public CDN instead of from your own site. |
| **Site CDN (Content Delivery Network) base url** | Optional. An address, such as `https://cdn.mysite.com`, placed in front of your site's own scripts and stylesheets so they are served by your CDN. It must be a full address. It is ignored when it is empty or when **Resource Debug Mode** is on. |
| **Resource Debug Mode** | **From environment — disabled in Production, enabled otherwise** (default), **Enabled — use debuggable version of resources**, or **Disabled — use minified version of resources**. The debuggable files are larger but easier for a developer to read; the minified files are smaller and load faster. |

## Turn the dynamic cache on or off

The **Dynamic Cache** feature (`OrchardCore.DynamicCache`, on by default in most sites) keeps a ready-made copy of parts of your pages, such as menus and widgets, so the site doesn't rebuild them on every visit. It has no screen of its own: you control it from the **Cache** tab.

1. Open **Settings > General** and click the **Cache** tab.
2. Pick a **Cache Mode**.
3. Click **Save**.

| Option | What it does |
| --- | --- |
| **From environment — enabled in Production, disabled otherwise** | The default. The cache is on for the live site and off on a developer's computer. |
| **Enabled** | The cache is always on. |
| **Enabled with cache debug mode** | The cache is on, and the page source contains notes that show which parts came from the cache. For developers. |
| **Disabled** | The cache is off. Every page is built from scratch, which is slower. |

!!! tip
    If you change a menu or a widget and the site still shows the old version, the change usually appears after the cached copy expires or is cleared. Turning the cache off for a moment helps you check whether caching is the cause.

## Change how the admin looks and behaves

**Settings > Admin** changes the admin for everyone who uses it.

1. Open **Settings > Admin**.
2. Check or clear the options, and pick the layouts.
3. Click **Save**.

| Field | What it does |
| --- | --- |
| **Enable theme toggler** | On by default. Shows a button in the top bar to switch the admin between light and dark. |
| **Enable Admin Menu filter** | Shows a filter box above the admin menu, to find a menu item by typing. |
| **Enable quick navigation** | On by default. Shows the quick navigation palette, opened with Ctrl+K (Cmd+K on a Mac), to jump to a screen or an item by typing. |
| **Display New menu** | Shows a **New** menu in the admin menu to create content items. |
| **Display titles in top bar** | Shows the title of the current screen in the top bar. |
| **Show breadcrumb** | On by default. Shows the breadcrumb trail (the path to the current screen) on admin screens. |
| **Navigation menu behavior** | **Focused** (default): only the section of the current page is open, and opening a section closes the others. **Persistent**: sections stay open until you close them, so several can be open at once. The section that holds the current page is always open. |
| **List layout** | How admin lists, such as the content items list, are shown: **List** or **Grid**. **Use configuration default** keeps the layout your IT team chose. See [Finding your way](../getting-started/finding-your-way.md). |
| **Actions layout** | How the actions of each row of a list, and the secondary actions at the bottom of edit screens, are shown: as **Buttons**, or in one compact **Menu**. On small screens the secondary actions are always in a menu. **Use configuration default** keeps the layout your IT team chose. |
| **Let users choose the layout of a list** | On by default. Each list shows a selector of the available layouts. A person's choice is remembered in their browser, per list, and applies only to them. The **List layout** above stays the one every list starts from. |
| **Recent content items in quick navigation** | Shown when the **Content Quick Navigation** feature (`OrchardCore.Contents.QuickNavigation`) is on. How many of the most recently updated content items the quick navigation palette looks at. The default is 50, and the value must be at least 1. Of those items, only the ones the person can edit and that have a title appear. |

The changes apply to everyone at the next page they open.

## See which template drew a part of a page

**Settings > Debugging** helps a site builder or designer who works on [templates](../design/templates-and-placements.md).

1. Open **Settings > Debugging**.
2. Check **Write shape debug information**.
3. Click **Save**.

From then on, each part of every page is wrapped in an HTML comment that names the template or the piece of code that produced it. You see these comments when you view the page source in your browser (usually right-click the page, then **View page source**). Visitors don't see them on the page, but anyone can read them in the source, so clear the option and click **Save** when you are done.

## What visitors see when something goes wrong

The **Orchard Diagnostics** feature (`OrchardCore.Diagnostics`) shows a friendly error page in the site's theme instead of a technical error. It has no settings. The pages and their messages are:

| Problem | Message on the page |
| --- | --- |
| The page doesn't exist (404) | *The page could not be found.* |
| The visitor must sign in (401) | *You are not authorized to view this content.* |
| The visitor isn't allowed (403) | *You do not have access to this resource.* |
| The request was malformed (400) | *Your browser sent a request that this server could not understand.* |
| Too many requests (429), see [rate limits](security.md#limit-how-many-requests-visitors-can-send) | *You have sent too many requests within a specified time frame.* |
| Any other error | *An error occurred while executing this request.* |

A designer can change how these pages look with a template; see [Templates and placements](../design/templates-and-placements.md). The details of an error are written to the site's logs, which your IT team reads.
