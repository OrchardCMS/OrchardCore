---
title: Lists and blogs
description: Use the List part to make a content item hold other content items, such as a blog and its posts, and manage, order and page those items.
technical_manual:
  - reference/modules/Lists/README.md
  - reference/modules/Feeds/README.md
---

# Lists and blogs

Some content belongs inside other content: blog posts inside a blog, episodes inside a podcast, jobs inside a careers page. The **List** part turns a content item into a **container** (the *list*) that holds content items of the types you choose (the *contained items*). The site shows the list's page with its items under it, a page at a time, and the admin gives you a screen to manage the items of each list.

| | |
| --- | --- |
| **Menu** | The list's content item: **Content > Content Items**, then **List ... Items** on its row. Settings: **Design > Content Definition > Content Types** |
| **Permission** | The content permissions of the list and of its contained types (for example *Edit content for others*, *Publish or unpublish content for others*); *Edit content types* to change the list settings |
| **Feature** | **Lists** (`OrchardCore.Lists`) |


## How the Blog recipe uses lists

A site set up with the **Blog** recipe has a ready-made list:

- The **Blog** content type has a **List** part whose contained type is **Blog Post**, with 10 items per page.
- One **Blog** content item exists. The **Blog** entry at the top of the admin menu opens it.
- **Blog Post** is not **Creatable** or **Listable**, so it doesn't appear in the **New** menu or in **Content > Content Items**: you create and find posts through the blog.
- The permalink of a post starts with the blog's own address, for example `/blog/my-first-post`.

## Open the items of a list

1. Open **Content > Content Items**.
2. On the row of the list (for example your blog), click **List ... Items** (for example **List BlogPost Items**), or **List Items** when the list holds several types.

On a Blog site, click **Blog** at the top of the admin menu instead.

The screen shows a bar with **Edit ...** (edit the list item itself) and the button to add an item, then the contained items, newest first unless ordering is on. Each row shows the item's title, type, status (**Published**, **Draft**) and when it was last modified, with **Edit**, **View** and an **Actions** menu, as in the content items list. When the list setting **Show container's header** is on, the list item is also shown in a box at the top.

### Search and filter the items

- Type in **Search** and press Enter to find items by their title.
- Use the filter button next to it: **Only draft items**, **Only published items** or **Owned by me**.

## Add an item to a list

1. Open the items of the list.
2. Click **Create ...** (for example **Create Blog Post**). When the list holds several content types, click **New** and pick the type.
3. Fill in the editor and click **Publish** or **Save Draft**.

The item is saved inside the list. You only see the types your role is allowed to edit.

!!! note
    Items are added to a list only from the list's screen (or its editor). An item created elsewhere doesn't join a list, and the editor has no box to move an existing item to another list.

## Reorder the items of a list

When the list's **Enable Ordering** setting is on, you choose the order of the items yourself.

1. Open the items of the list.
2. Drag an item by the handle at the start of its row to its new place.

The order is saved as soon as you drop the item. The site shows the items in that order. Dragging is offered only while the list shows all items: clear the search box and the draft or published filter first. New items are added at the end.

When ordering is off, the newest items come first.

## Edit or delete items

Use the buttons of each row, exactly as in the content items list: **Edit**, **View**, and in **Actions**: **Publish Draft** or **Unpublish**, **Clone**, **Delete** and the other actions your site offers. See [Content items](content-items.md).

## Make a content type a list

1. Open **Design > Content Definition > Content Types** and click **Edit** on the type that should hold items (create a type for the items first if needed).
2. Click **Add Parts**, click **Add & Configure** on the **List** row.
3. Fill in the settings below and click **Save**.

| Setting | What it does |
| --- | --- |
| **Page Size** | The number of items shown per page, on the site and in the admin screen of the list. 10 by default. |
| **Enable Ordering** | Lets editors drag items into the order they want. When you turn it on, the existing items get a starting order. |
| **Contained Content Types** | Required. The types this list can hold, for example **Blog Post** for a blog. Tick at least one. |
| **Show container's header** | Shows the list item itself at the top of the list screen in the admin. |
| **Show full pager** | Uses a pager with numbered pages instead of only previous and next buttons. It costs the site one more database query per page. |

Usually you also untick **Creatable** and **Listable** on the contained type, so its items are created and found only through their list, as the Blog recipe does. To give contained items addresses under the list, use the **Autoroute** container options; see [URLs and homepage](urls-and-homepage.md).

## RSS feed of a list

When the **Feeds** feature is on, the editor of a list item has feed options: **Disable RSS feed link generation.**, **Feed proxy Url** (a public address that relays the feed) and **Feed items count** (how many items the feed holds). The hint under **Feed proxy Url** shows the address of the list's feed. See [Feeds](../search/feeds.md).

## Add lists to the admin menu

To give editors a menu entry for each list, add a **Lists** node to an admin menu: it adds a link for each content item of the type you choose. See [Admin menu](../design/admin-menu.md).

!!! example "See it in practice"
    [Build a blog](../use-cases/build-a-blog.md) walks through this step by step.
