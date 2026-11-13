---
title: SEO
description: Control how pages appear in search results and when shared on social networks, and decide what search engine robots may visit.
technical_manual:
  - reference/modules/Seo/README.md
  - reference/modules/CustomSettings/README.md
  - reference/modules/Sitemaps/README.md
---

# SEO

Search engine optimization (SEO) is about helping search engines and social networks show your pages well. Orchard Core gives you three tools:

- The **SEO** part on a content item: its page title, meta description, canonical address, robots instructions, and the title, text and picture used when the page is shared on social networks.
- **Social Meta Settings**: site-wide defaults used when a content item doesn't fill in its own values.
- The **Robots** settings: the `robots.txt` file that tells search engine robots what they may visit.

| | |
| --- | --- |
| **Menu** | The editor of a content item with the **SeoMeta** part; **Settings > Social Meta Settings**; **Settings > Search > Search Engine Optimization > Robots** |
| **Permission** | The permission to edit the content item; *Manage Custom Settings - Social Meta Settings*; *Manage SEO related settings* |
| **Feature** | **SEO** (`OrchardCore.Seo`); **Custom Settings** (`OrchardCore.CustomSettings`) for **Social Meta Settings** |


## Add SEO options to a content type

1. Open **Design > Content Definition > Content Types** and click **Edit** on the type, for example **Article**.
2. Click **Add Parts**, then **Add & Configure** on the **SeoMeta** row.
3. Tick the panels editors should see, then click **Save**.

| Setting | What it does |
| --- | --- |
| **Display Keywords** | Shows the **Meta Keywords** box. |
| **Display Custom Meta Tags** | Shows the **Custom Meta Tags** table. |
| **Display Open Graph** | Shows the **Open Graph** panel (sharing on Facebook, LinkedIn and others). |
| **Display Twitter** | Shows the **Twitter** panel (sharing on X). |
| **Display Google Schema** | Shows the **Google Schema** panel. |

All five are unticked when you add the part, so editors see only the basic boxes until you tick them.

## Fill in the SEO of a content item

1. Open the content item's editor and click the **SEO** tab.
2. Fill in the boxes you need. Leave a box empty to use the default.
3. Click **Publish** (or **Save Draft**).

| Field | What it does |
| --- | --- |
| **Render** | Ticked by default. Untick it to render no SEO tags at all for this page. |
| **Page Title** | Replaces the title shown in the browser tab and in search results. Keep it under 70 characters. When empty, the site's usual title is used. |
| **Meta Description** | The short text search engines may show under the title. When empty, the **Default Meta Description** of the Social Meta Settings is used. |
| **Meta Keywords** | Keywords for the page, separated by commas. Shown only when **Display Keywords** is ticked. |
| **Canonical** | Replaces the page's canonical address (the main address when the same content is reachable at several). When empty, the page's own address is used. It must not contain spaces or characters such as `?`, `#`, `&`, `%` or `=`, otherwise saving fails with *The canonical entry contains invalid characters.* |
| **Meta Robots** | Instructions for search engine robots for this page, for example `noindex, nofollow` to keep it out of search results. |
| **Custom Meta Tags** | Extra meta tags. Click **Add a custom meta tag** and fill in **Name**, **Property**, **Content**, **Http Equiv** or **Charset** as needed. |
| **Default social image**, **Open graph image**, **Twitter image** | Pictures from the media library used when the page is shared. The default social image is used when the network's own image is empty. |

### Open Graph, Twitter and Google Schema panels

These panels appear in the **SEO** tab when the part settings tick them. Every box falls back to a default when empty.

| Field | When empty, uses |
| --- | --- |
| **Open Graph Type** | The **Open Graph Type** of the Social Meta Settings. |
| **Open Graph Title** | The **Page Title**, then the content item's title. |
| **Open Graph Description** | The **Meta Description**. |
| **Twitter Title** | The **Page Title**, then the content item's title. |
| **Twitter Description** | The **Meta Description**. |
| **Twitter Card**, **Twitter Creator**, **Twitter Site** | The matching values of the Social Meta Settings. |
| **Google Schema** | The **Google Schema** of the Social Meta Settings. Type structured data (JSON-LD) here; it must be valid JSON, otherwise saving fails with *The google schema is written in an incorrect format.* |

## Set site-wide sharing defaults

**Social Meta Settings** holds the defaults used by every content item with the SEO part. The menu appears when the **Custom Settings** feature is on.

1. Open **Settings > Social Meta Settings**.
2. In the **Content** section, fill in the default texts.
3. In the **Social** section, pick the default pictures.
4. Click **Save**.

| Field | What it does |
| --- | --- |
| **Default Meta Description** | Used by pages with no meta description. |
| **Open Graph Type** | For example `website` or `article`. |
| **Default Open Graph Description** | Used when a page has no Open Graph description and no meta description. |
| **Open Graph Site Name** | The site name shown on shared links. When empty, the site name from **Settings > General** is used. |
| **Open Graph App Id** | The Meta (Facebook) app id, if you have one. |
| **Twitter Card** | The card style on X, for example `summary` or `summary_large_image`. |
| **Default Twitter Description** | Used when a page has no Twitter description and no meta description. |
| **Twitter Creator**, **Twitter Site** | The X handles of the author and of the site, for example `@yourcompany`. |
| **Google Schema** | Default structured data for search engines. |
| **Default social image**, **Open graph image**, **Twitter Image** | Default pictures for shared links. |

Permission: *Manage Custom Settings - Social Meta Settings*. See [Custom settings](../settings/custom-settings.md).

## Set up robots.txt

Search engine robots read `https://your-site/robots.txt` to learn what they may visit. Orchard Core builds this file from these settings.

1. Open **Settings > Search > Search Engine Optimization > Robots**.
2. Set the options and click **Save**.

| Setting | What it does |
| --- | --- |
| **Add a rule to allow all web robots.** | Adds `User-agent: *`, so the rules apply to every robot. |
| **Add a rule to disallow web robots to the admin routes.** | Adds `Disallow: /Admin` (or your admin address), so robots don't visit the admin. |
| **Additional rules** | Any other lines to add, typed exactly as they should appear in the file. |
| **Include all sitemaps.** | Shown when the **Sitemaps** feature is on. Adds the address of each sitemap, so robots find them. See [Sitemaps](../search/sitemaps.md). |

Open `/robots.txt` on the site to check the result.

!!! note
    If your developer has placed a real `robots.txt` file in the site's web folder, that file is served instead and these settings have no effect.

Permission: *Manage SEO related settings*.
