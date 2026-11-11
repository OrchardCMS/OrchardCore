---
title: RSS feeds
description: Offer an RSS feed for a blog or any other list, choose how many items it shows, point it to a feed service, or turn it off.
technical_manual:
  - reference/modules/Feeds/README.md
  - reference/modules/Lists/README.md
---

# RSS feeds

An RSS feed lets people follow your site in a feed reader or another website: each time you publish a new post, it shows up in their reader. Orchard Core gives every list content item, such as a blog, its own RSS feed of the items it contains. You don't create feeds: you turn the feature on and adjust each list's feed in its editor.

| | |
| --- | --- |
| **Menu** | The editor of a list content item, for example **Content > Content Items**, then **Edit** on your blog |
| **Permission** | Permission to edit that content item, see [Content items](../content/content-items.md) |
| **Feature** | **Feeds** (`OrchardCore.Feeds`), with **Lists** (`OrchardCore.Lists`) |

The **Blog** recipe turns on **Feeds**, so a new blog already has a feed. See [Lists and blogs](../content/lists-and-blogs.md) for lists themselves.

## Find the feed of a list

1. Open the list's editor, for example your **Blog**.
2. Look under **Feed proxy Url**: the hint *The current feed is available at ...* shows the feed's address, as a link.

The address has the form `https://www.example.com/Contents/Lists/<id>/rss`, where `<id>` is the list's content item id. The list's page on the site also announces the feed to browsers and feed readers, so visitors can subscribe by entering the address of the list page, such as `https://www.example.com/blog`.

## What the feed contains

- The feed's title is the list's title, and its description is the list's body text.
- It shows the **published** items of the list, newest first (by creation date).
- Each item has its title, a link to its page and its body text.
- Drafts and unpublished items never appear.

## Change the feed of a list

1. Open the list's editor.
2. Change the feed settings, described below.
3. Click **Publish** (or **Save Draft** and publish later).

| Field | What it does |
| --- | --- |
| **Disable RSS feed link generation.** | Turns the feed off for this list: the page stops announcing it, and the feed address shows "not found". |
| **Feed proxy Url** | Optional. The address of an outside feed service (for example one that counts subscribers) that republishes this feed. When filled in, the list page announces this address instead of the site's own feed. The site's feed keeps working at its own address, which the service reads. |
| **Feed items count** | How many items the feed shows. The default is 20. With `0`, the feed has no items. |

## Turn off all feeds

Turn off the **Feeds** feature in **Tools > Features**, see [Features](../getting-started/features.md). The feed settings disappear from list editors and no feed is served.

!!! note
    Only lists have feeds. Other pages, such as taxonomy term pages or search results, don't offer one.
