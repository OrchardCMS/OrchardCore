---
title: Admin menus
description: Add your own entries to the admin menu, such as shortcuts to a content list, a blog or any admin page, and choose who sees them.
technical_manual:
  - reference/modules/AdminMenu/README.md
---

# Admin menus

The admin menu on the left of every admin page is built by the features you turn on. With **Admin Menus** you can add your own entries to it: a shortcut to the articles list, a link to each blog, a group of links for one team, or a link to any admin page. Each set of entries is an **admin menu** made of **nodes**, and you can turn a whole admin menu on or off and decide which roles see it.

| | |
| --- | --- |
| **Menu** | **Tools > Admin Menus** |
| **Permission** | *Manage the admin menu* |
| **Feature** | **Admin Menu** (`OrchardCore.AdminMenu`) |


## See the admin menus

Open **Tools > Admin Menus**. Each row shows the admin menu's name and a status badge, **Enabled** or **Disabled**, with the buttons **Edit Nodes**, **Edit** and **Delete**, and an **Actions** menu with **Disable** or **Enable**. Type in **Search** and press Enter to filter by name. To delete several admin menus, tick them and choose **Delete** from the **Actions** menu at the top.

The **Blog** recipe creates an admin menu that adds shortcuts such as **Blog** and **Main Menu** at the top of the admin menu.

## Create an admin menu

1. Open **Tools > Admin Menus** and click **Add Admin Menu**.
2. Type a **Name**. It is required. The name also names the permission that lets people see this admin menu.
3. Click **Create**.

The new admin menu is empty. Click **Edit Nodes** to add entries. To rename it later, click **Edit**, change the **Name** and click **Save**.

## Add a node

1. On the admin menu's row, click **Edit Nodes**. The page is titled **Edit Nodes for '*name*'**.
2. Click **Add Node**. The **Available Admin Nodes** window lists the node types.
3. Click **Add** on the type you want.
4. Fill in the node's fields (see below) and click **Create**.

The node appears in the tree. Open the admin menu on the left to see it; entries show only when the admin menu is enabled and has at least one node.

### Node types

| Node type | What it adds |
| --- | --- |
| **Link Admin Node** | *Adds a link with a text and a url.* |
| **Placeholder Admin Node** | *Adds a menu item that acts as a container for others.* A heading that opens to show the nodes you put inside it. |
| **Content Types** | *Add a link for each one of the selected content types.* Each link opens the content items list of that type. Comes from the **Contents** feature. |
| **Lists** | *Adds links for list content items* One link for each content item of a type that holds a list, such as each blog. Comes from the **Lists** feature. |

#### Link Admin Node

| Field | What it does |
| --- | --- |
| **Link Text** | Required. The text shown in the admin menu. |
| **Link Url** | Required. The address. An address such as `Contents/ContentItems/Article` is taken from the start of the admin, so it opens `/Admin/Contents/ContentItems/Article`. Addresses that start with `/` or `https://` are used as they are. |
| **Target** | Where the link opens: the same window (the default) or a new tab, for example. |
| **Icon** | Click **Pick** to choose an icon in **Pick an icon**. It is shown when the link is at the first level of the menu. |
| **Permissions** | Optional. The link shows only to people who have **all** the permissions you pick. |

!!! tip
    The easiest way to get a link address: open the admin page you want, copy the part of the address after `/Admin/`, and paste it in **Link Url**.

#### Placeholder Admin Node

It has a **Link Text** (required), an **Icon** and **Permissions**, but no address. Add other nodes and drag them inside it.

If a placeholder or link has the same text as an existing section of the admin menu, such as **Content**, its nodes are added to that section instead of creating a second one.

#### Content Types

| Field | What it does |
| --- | --- |
| **Show All Content Types** | Adds a link for every content type you can list. |
| **Default Icon** | The icon shared by all the links. |
| **Select Content Types** | Otherwise, tick the content types to add, and pick an **Icon** for each. |

#### Lists

| Field | What it does |
| --- | --- |
| **Content type** | The type whose content items get a link each, such as **Blog**. |
| **Icon for the content items** | The icon of each link. |
| **Add parent menu item** | Ticked: the links are grouped under a heading with the type's name. |
| **Icon for the parent menu item** | The icon of that heading. |

## Arrange, edit and turn off nodes

On **Edit Nodes for '*name*'**, each node has:

| Button | What it does |
| --- | --- |
| **Edit** | Opens the node's fields, plus **Selection Priority** and **Position Order**. Click **Save**. |
| **Delete** | Removes the node and the nodes inside it, after you confirm. |
| Arrow next to **Delete**, then **Disable** / **Enable** | Hides or shows the node without deleting it. Disabled nodes are greyed out. |

Drag a node up or down to change its order, or into another node to put it inside. The change is saved at once.

| Field on **Edit** | What it does |
| --- | --- |
| **Position Order** | *The position order of this link in the admin menu.* Use it to place the entry among the built-in ones. |
| **Selection Priority** | When two links point to the same page, the one with the higher value is highlighted as the current page. |

A node whose type comes from a feature that has been turned off shows *Can't show the item of type: ... Maybe a required module has been disabled?* with only a **Delete** button.

Click **Back** to return to the list of admin menus.

## Turn an admin menu off

On **Tools > Admin Menus**, open the row's **Actions** menu and click **Disable**. Its entries disappear from the admin menu for everyone, and the badge shows **Disabled**. Click **Enable** to bring them back.

## Choose who sees an admin menu

Each admin menu creates a permission named *View Admin Menu - name*. Only people whose role has that permission see its entries. *View Admin Menu - View All* gives access to every admin menu, and *Manage the admin menu* includes both.

Seeing an entry doesn't give access to the page it opens: people still need the permissions of that page. Use the **Permissions** box of a node to hide links people can't use anyway. Set the permissions in [Roles](../users/roles.md).
