---
title: Titles, URLs and the home page
description: Give content items a title and a web address, change or refresh a permalink, choose the home page, and use aliases.
technical_manual:
  - reference/modules/Title/README.md
  - reference/modules/Autoroute/README.md
  - reference/modules/Alias/README.md
  - reference/modules/HomeRoute/README.md
  - reference/modules/Liquid/README.md
---

# Titles, URLs and the home page

Each page or post of the site needs a name and an address that visitors can open, such as `https://www.example.com/about-us`. This page covers the **Title** of a content item, its **Permalink** (its address), how to make an item the home page of the site, and the **Alias**, a fixed name that templates use to find an item.

| | |
| --- | --- |
| **Menu** | In the editor of any content item whose type has the **Title**, **Autoroute** or **Alias** part |
| **Permission** | Permission to edit the content item. *Set homepage.* to make an item the home page (given to the **Administrator** role by default). |
| **Feature** | **Title** (`OrchardCore.Title`), **Autoroute** (`OrchardCore.Autoroute`), **Alias** (`OrchardCore.Alias`), **Home Route** (`OrchardCore.HomeRoute`) |

## Give a content item a title

Type the name of the item in **Title**, at the top of the editor. The title is what the admin lists show and, usually, the heading of the page on the site. It is also used to build the item's address (see below).

Depending on the content type, the title can be:

- **Editable**: you type it, and you may leave it empty.
- **Required**: you must type it. Saving without it shows *A value is required for Title.*
- **Generated**: the box is greyed out or hidden, and the site builds the title for you from other boxes, for example from a date and a name. The hint *The title will be auto-generated using the pattern.* tells you so.

## Set the permalink of a content item

The **Permalink** is the address of the item on the site, after the site's address. For example, the permalink `blog/summer-sale` gives `https://www.example.com/blog/summer-sale`. When the site's base address is set in **Settings > General**, it is shown before the box.

1. Leave **Permalink** empty to let the site build it from the title, for example *Summer Sale* gives `summer-sale`.
2. Or type the address you want, such as `offers/summer-sale`.
3. Save or publish the item.

The permalink is filled in when you first save the item. If another item already uses the address, the site adds a number to make it unique. Once the item is published, the arrow button next to the box opens the page on the site.

Rules for a permalink you type:

- Don't use spaces, two slashes in a row, or any of these characters: `: ? # [ ] @ ! $ & ' ( ) * + , . ; = < > \ | %`. Use dashes or underscores instead of spaces.
- Keep it to 1024 characters or fewer.
- It must be unique: *Your permalink is already in use.* means another item has it.
- It can't be `/`. To make an item the home page, use **Set as homepage** (see below).

When the box is greyed out, the content type doesn't allow custom addresses: the site always builds them.

### Change the address of a published item

Changing the title of a published item doesn't change its address: links to the page keep working. To build a fresh address from the changed title:

1. Tick **Refresh the path**.
2. Publish the item.

!!! warning
    The old address stops working. Links to it from other sites, bookmarks and search engines lead nowhere. Update your menus and links.

You can also type a different address in **Permalink** yourself, with the same effect. **Refresh the path** is shown only when the content type allows it.

### Turn off the address of an item

When the content type allows it, a **Disabled** checkbox appears above **Permalink**. Tick it to give the item no address of its own, for example for an item that is only shown inside another page.

### Addresses of items inside a list

Some items live inside another, such as the posts of a blog or the items of a list. The content type can let their addresses follow the address of their container:

| Option | Where it appears | What it does |
| --- | --- | --- |
| **Route contained items** | On the container, such as the blog | Gives the items inside it addresses under the container's address. |
| **Absolute** | On an item inside the container | Gives the item an address of its own, not under the container's, for example `https://mysite.com/my-child-item` instead of `https://mysite.com/my-parent/my-child-item`. |

See [Lists and blogs](lists-and-blogs.md).

## Choose the home page

The home page is what visitors see at the address of the site, such as `https://www.example.com/`. Any published content item with a permalink can be the home page.

1. Open the item you want as the home page.
2. Tick **Set as homepage**.
3. Click **Publish**.

The item becomes the home page once it is published. When you open it again, the editor shows *This content item is the current homepage* instead of the checkbox. To use another item as the home page, set that item as the home page: there can be only one.

**Set as homepage** appears only for content types that allow it, and only for people with *Set homepage.*. The home page itself is stored in the site settings by the **Home Route** feature, which has no screen of its own. A theme or module can also choose the home page, in which case your developer tells you how it works.

## Use an alias

An alias is a fixed name for a content item, such as `main-menu` or `footer-text`. It isn't an address: visitors never see it. Templates and site builders use it to find one particular item, whatever its title, for example to show the main menu in the theme.

1. Type a short name in **Alias**, for example `footer-text`.
2. Save the item.

When the content type has a pattern, leave **Alias** empty to build it again from the pattern. An alias must be unique (*Your alias is already in use.*) and can be up to 735 characters. A greyed-out box means the alias is always built for you.

!!! warning
    Don't change the alias of an existing item unless you know where it is used. A template that looks for the old alias no longer finds the item.

## Set up titles, permalinks and aliases (site builders)

| | |
| --- | --- |
| **Menu** | **Design > Content Definition > Content Types**, then **Edit** on the type, then **Edit** next to the part |
| **Permission** | *Edit content types.* |

Open the content type, click **Edit** next to the **Title**, **Autoroute** or **Alias** part, change the settings and click **Save**. To add one of these parts to a type, see [Content types](content-types.md).

### Title settings

| Setting | What it does |
| --- | --- |
| **Behavior option** | **Title is editable**, **Title is editable and required**, **Title is generated and input is disabled**, or **Title is generated and input is hidden**. |
| **Pattern** | Required for the generated options. A Liquid template that builds the title, for example from fields of the item. Ask a developer for help with Liquid. |
| **Placeholder** | Grey example text in the empty **Title** box. |
| **Render Title** | Shows the title on the page on the site. |

### Autoroute settings

| Setting | What it does |
| --- | --- |
| **Pattern** | How the address is built, as a Liquid template. The default turns the title into lowercase words joined by dashes (see below). |
| **Allow custom path** | Lets editors type their own **Permalink**. When it's off, the box is greyed out. |
| **Allow path updates** | Shows **Refresh the path** on items. |
| **Show homepage options** | Shows **Set as homepage** on items, to people with *Set homepage.*. |
| **Allow disabled** | Shows the **Disabled** checkbox on items. |
| **Allow contained item routing** | Set on the container type: shows **Route contained items** on its items. |
| **Manage contained item routes** | Set on the type of the items inside the container: lets the container give them addresses. |
| **Allow absolute path** | Set on the type of the items inside the container: shows **Absolute** on them. |

The default **Pattern** is:

```liquid
{{ ContentItem.DisplayText | slugify }}
```

To put every item of the type under a fixed start, add it before the pattern, for example `blog/` followed by the default pattern gives addresses such as `blog/summer-sale`.

### Alias settings

| Setting | What it does |
| --- | --- |
| **Pattern** | A Liquid template that builds the alias when it is left empty. |
| **Options** | **Alias is editable**, or **Alias is generated and input is disabled**. |

!!! example "See it in practice"
    [Build a landing page](../use-cases/landing-page-with-widgets.md) walks through this step by step.
