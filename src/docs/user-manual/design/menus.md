---
title: Menus
description: Build the navigation menus of your site, add links to pages and other addresses, nest and reorder them, and show items only to some people.
technical_manual:
  - reference/modules/Menu/README.md
  - reference/modules/Navigation/README.md
---

# Menus

A menu is the list of links visitors use to move around your site, usually shown in the navigation bar at the top. In Orchard Core a menu is a content item of type **Menu**, so you edit, save as a draft and publish it like other content. The **Blog** and **Agency** recipes create a **Main Menu**, which the built-in themes show.

| | |
| --- | --- |
| **Menu** | **Content > Menus** |
| **Permission** | *Manage menus* to add, edit and delete menu items; the *List Menu content items* permission to see the menu entry |
| **Feature** | **Menu** (`OrchardCore.Menu`), which uses **Navigation** (`OrchardCore.Navigation`, always on) |


## See your menus

Open **Content > Menus**. It is the content items list, filtered to the **Menu** type, with each menu's title, status and actions. Click a menu's title or **Edit** to open it. The **Blog** recipe also adds a shortcut to the **Main Menu** at the top level of the admin menu.

## Create a menu

1. Open **Content > Menus** and click **New Menu**.
2. Type a **Title**, such as *Footer Menu*.
3. Leave **Alias** empty to have it made from the title (for example `footer-menu`), or type your own. The theme finds a menu by its alias.
4. Click **Publish** (or **Save Draft**).

**Add Menu Item** is unavailable until the menu has been saved once.

## Add a menu item

1. Open the menu. Its editor shows **Title**, **Alias**, an **Add Menu Item** button and the current items.
2. Click **Add Menu Item**. The **Available Menu Items** window lists the item types.
3. Click **Add** on the type you want (see below).
4. Fill in the fields and click **Publish** at the bottom of the item's form. You return to the menu.
5. On the menu, click **Publish** to show the change on the site.

!!! warning "Publish the menu after changing its items"
    The **Publish** button of a menu item only saves the item into a draft of the menu. The same goes for editing or deleting an item. Visitors keep seeing the published menu until you click **Publish** on the menu itself.

### Menu item types

| Type | Use it for |
| --- | --- |
| **Link Menu Item** | *Links to a custom url.* Any address: a page of your site such as `~/about`, a section such as `~/blog`, or another website such as `https://example.com`. |
| **Content Menu Item** | *Links to a Content url.* A content item that you pick. The link follows the item if its address changes. |
| **Html Menu Item** | *Links to a custom url with HTML.* Like a link, but the text can contain HTML, such as an icon. |

#### Link Menu Item

| Field | What it does |
| --- | --- |
| **Name** | The text of the link in the menu. |
| **Url** | The address. Start addresses of your own site with `~/`, such as `~/contact`, so they keep working if the site moves to another address. An address the site can't accept is refused with *... is an invalid url.* |
| **Target** | Where the link opens. *Opens the linked document in the same frame as it was clicked (this is default).* or *Opens the linked document in a new window or tab.* are the usual choices. |
| **Permissions** | Optional. See [Show a menu item only to some people](#show-a-menu-item-only-to-some-people). |

#### Content Menu Item

| Field | What it does |
| --- | --- |
| **Name** | The text of the link in the menu. |
| **Check content permissions** | Ticked: the item is shown only to visitors allowed to view the picked content item. This makes the menu slower to build, so leave it off unless you need it. |
| **Selected Content Item** | Required. Pick the content item to link to. |

#### Html Menu Item

It has the same **Name**, **Url**, **Target** and **Permissions** fields as a link, plus **Html**: the HTML shown instead of the name, inside the link. By default the HTML is cleaned to remove scripts; the **Sanitize Html** setting of the **Html Menu Item** part in the content type editor controls this.

## Nest, reorder and change items

The menu editor shows the items as a list, with sub-items indented under their parent. Each item shows its name, its address or type, and three buttons:

| Button | What it does |
| --- | --- |
| **Edit** | Opens the item's fields. Click **Publish** on the item to go back to the menu (the menu itself still needs publishing). |
| **Add** | Adds a sub-item under this item, which themes show as a drop-down. Pick the type in **Available Menu Items**. |
| **Delete** | Removes the item and its sub-items, after you confirm. |

Drag an item up or down to reorder it. While dragging, move it to the right to nest it under the item above, or to the left to take it out of its parent. Its sub-items move with it. When you leave the page after dragging, the browser warns *You have reordered the navigation.* Click **Publish** (or **Save Draft**) to keep the new order.

## Show a menu item only to some people

Link and Html menu items have a **Permissions** box: *The permissions required to display this menu item (optional).* Pick one or more permissions; the item is shown only to visitors who have **all** of them. For example, pick *Access admin panel* to show an "Admin" link to staff only. Leave it empty to show the item to everyone.

For content menu items, use **Check content permissions** instead.

## How the menu appears on the site

The theme decides where a menu appears and how it looks. The Blog, Agency and Default themes show the menu whose alias is `main-menu` in their navigation bar, with sub-items as drop-downs. To show another menu, for example in the footer, a theme or template must ask for it by its alias; see [Templates](templates-and-placements.md).

These themes keep the built menu in a cache, separately for each combination of roles, and refresh it when you publish the menu. Items with permissions are therefore shown correctly to each visitor.

!!! example "See it in practice"
    [Build a blog](../use-cases/build-a-blog.md) walks through this step by step.
