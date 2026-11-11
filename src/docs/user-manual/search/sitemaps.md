---
title: Sitemaps
description: Publish XML sitemaps and sitemap indexes so search engines find your pages, choose what each sitemap lists, and clear the sitemap cache.
technical_manual:
  - reference/modules/Sitemaps/README.md
  - reference/modules/ContentLocalization/README.md
---

# Sitemaps

A sitemap is an XML file, such as `https://www.example.com/sitemap.xml`, that lists the pages of your site for search engines like Google and Bing. Orchard Core builds sitemaps for you from your content: you create a sitemap, add **sources** that say what it lists (content types, or fixed paths), and the site keeps it up to date as you publish. A **sitemap index** groups several sitemaps for large sites.

| | |
| --- | --- |
| **Menu** | **Tools > Search Engine Optimization > Sitemaps**, **Sitemap Indexes** and **Sitemaps Cache** |
| **Permission** | *Manage sitemaps*. Administrators have it by default. |
| **Feature** | **Sitemaps** (`OrchardCore.Sitemaps`) |


!!! warning "Set the base URL first"
    Sitemaps contain full addresses, so they need the site's address. If the sitemap screens show *The site settings 'Base url' property is empty, but is required to generate sitemaps.*, open **Settings > General** and fill in **Base url**, see [General settings](../settings/general.md).

## Create a sitemap

1. Open **Tools > Search Engine Optimization > Sitemaps** and click **Add Sitemap**.
2. Type a **Name**.
3. Type a **Permalink**, or leave it empty to build one from the name.
4. Leave **Enabled** ticked.
5. Click **Create**.
6. Click **Manage Sources** in the sitemap's row and add at least one source (see the next sections). A sitemap without sources is empty.

| Field | What it does |
| --- | --- |
| **Name** | Required. The name shown in the admin. |
| **Permalink** | The address of the sitemap after the site's base URL, for example `sitemap.xml`. It must end with `.xml`, can't be the home page, can't contain spaces or special characters, and must not be used by another sitemap. Leave it empty to generate it from the name. |
| **Enabled** | Untick to stop serving the sitemap at its address without deleting it. |

## List content items in a sitemap

1. Open **Tools > Search Engine Optimization > Sitemaps** and click **Manage Sources** in the sitemap's row.
2. Click **Add Source**.
3. In **Available Sources**, click **Add** under **Sitemap Content Types**.
4. Choose which content types to list (see below).
5. Click **Create**.

The sitemap lists the published content items of the chosen types, with their address. The source lists only content types that have an address of their own, that is with the **Autoroute** part (see [URLs and the home page](../content/urls-and-homepage.md)).

| Field | What it does |
| --- | --- |
| **Index All Content Types** | Lists every content type that has an address. When ticked, set one **Change Frequency** and **Priority** for all of them. |
| **Limit Items** | Lists only part of one content type, to keep large sitemaps small. Pick one type, then type **Skip** (how many items to leave out first) and **Take** (how many to list). |
| **Select Content Types** | When neither box above is ticked, tick each content type to list and set its own **Change Frequency** and **Priority**. |
| **Change Frequency** | Tells search engines how often the pages change: **Always**, **Hourly**, **Daily**, **Weekly**, **Monthly**, **Yearly** or **Never**. |
| **Priority** | How important the pages are compared with other pages of your site, from **0.0** to **1.0**. |

!!! tip "Very large sites"
    Search engines accept a limited number of addresses in one sitemap. Split big content types over several sitemaps with **Limit Items** (for example the first items in one sitemap and the rest in another) and group them with a [sitemap index](#group-sitemaps-in-a-sitemap-index).

## Add a fixed address to a sitemap

For a page that isn't a content item, such as a page built by a developer:

1. Open the sitemap's sources with **Manage Sources** and click **Add Source**.
2. Click **Add** under **Sitemap Custom Path**.
3. Type the **Path**, for example `contact/office-hours`, without the site address.
4. Choose the **Change Frequency** and **Priority**.
5. Click **Create**.

| Field | What it does |
| --- | --- |
| **Path** | Required. The path to list, after the site's address. No spaces or consecutive slashes; use dashes or underscores. |
| **Change Frequency** | How often the page is likely to change. |
| **Priority** | How important the page is compared with other pages, from **0.0** to **1.0**. |

## Manage the sources of a sitemap

On the **Manage Sources** screen, each source has **Edit** and **Delete** buttons. Click **View** at the top to open the sitemap in a new tab and check what it lists, and **Back** to return to the list.

## Manage sitemaps

In **Tools > Search Engine Optimization > Sitemaps**, each row shows the sitemap's name and **Status** (**Enabled** or **Disabled**), with these buttons:

- **View** opens the sitemap as search engines see it.
- **Manage Sources** opens its sources.
- **Edit** changes its name, permalink or **Enabled** box.
- **Delete** removes it after you confirm.
- The **Actions** menu offers **Disable** or **Enable**.

Type in **Search** to filter the list. To delete several sitemaps, tick them and choose **Delete** in the **Actions** menu above the list.

## Group sitemaps in a sitemap index

A sitemap index is one address that points search engines to several sitemaps.

1. Open **Tools > Search Engine Optimization > Sitemap Indexes** and click **Add Sitemap Index**.
2. Type a **Name** and a **Permalink**, for example `sitemap-index.xml`.
3. Tick the sitemaps it contains under **Contained Sitemaps**.
4. Leave **Enabled** ticked and click **Save**.

| Field | What it does |
| --- | --- |
| **Name** | Required. The name shown in the admin. |
| **Permalink** | Required. The address of the sitemap index; it must end with `.xml`. |
| **Enabled** | Untick to stop serving the sitemap index. |
| **Contained Sitemaps** | The sitemaps listed in the index. |

The list of sitemap indexes has the same **View**, **Edit**, **Delete**, **Enable** and **Disable** actions as sitemaps.

## Tell search engines about your sitemaps

Give the sitemap or sitemap index address to the search engines' webmaster tools (such as Google Search Console). With the **SEO** feature on, you can also list every sitemap in your `robots.txt` file: tick **Include all sitemaps.** in **Settings > Search > Search Engine Optimization > Robots**, see [SEO](../content/seo.md).

## Exclude or adjust one content item

Add the **Sitemap** part (`SitemapPart`) to a content type to control each item, see [Content types](../content/content-types.md). The item's editor then shows:

| Field | What it does |
| --- | --- |
| **Override sitemap configuration** | Tick to use the settings below for this item instead of the sitemap source's. |
| **Exclude** | Leaves this item out of every sitemap. |
| **Change Frequency** | How often this item is likely to change. |
| **Priority** | How important this item is, from **0.0** to **1.0**. |

## Clear the sitemap cache

The site builds each sitemap the first time it is requested and keeps the result in a cache file. Publishing or unpublishing content, or changing a sitemap, updates it. If a sitemap looks out of date, clear its cache:

1. Open **Tools > Search Engine Optimization > Sitemaps Cache**.
2. Click **Purge** in the row of a cached file, or **Purge All** to clear every one, and confirm.

The next visit to the sitemap builds it again. Type in **Search** to find a file by name.

**Sitemaps Cleanup** (`OrchardCore.Sitemaps.Cleanup`) adds a background task, **Sitemap Cache Cleaner**, that runs every five minutes and deletes cache files that no longer belong to a sitemap. See [Background tasks](../settings/background-tasks.md).

## Sitemaps for multilingual sites

With **Localized Content Item Sitemaps** (`OrchardCore.ContentLocalization.Sitemaps`), each translated content item in a sitemap also lists the addresses of its translations, so search engines show visitors the page in their language. It needs content localization, see [Localization](../content/localization.md).

## Sitemaps for decoupled Razor Pages

**Sitemaps for Decoupled Razor Pages** (`OrchardCore.Sitemaps.RazorPages`) is for sites whose pages a developer builds as Razor Pages. The developer sets up which content types map to which pages; those types then appear in the **Sitemap Content Types** source. It has no screen of its own.

## Copy sitemaps to another site

Add the **All Sitemaps** step to a deployment plan to export every sitemap, see [Import and export](../administration/import-and-export.md).
