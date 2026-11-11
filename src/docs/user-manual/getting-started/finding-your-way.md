---
title: Find your way around
description: Get around the admin, with the dashboard, the admin menu, the top bar, quick navigation, admin lists and the messages shown after you save.
technical_manual:
  - reference/modules/Admin/README.md
  - reference/modules/AdminDashboard/README.md
  - reference/modules/Contents/README.md
  - reference/modules/Themes/README.md
---

# Find your way around

The admin is the part of the site where you manage it: you write content, upload media, add users and change settings. This page shows how the admin screens are laid out and how to move around in them, so the rest of the manual makes sense.

| | |
| --- | --- |
| **Menu** | Everything under the admin, at `/Admin` on your site |
| **Permission** | *Access admin panel* to open the admin at all. It is given by default to the **Administrator**, **Editor**, **Moderator**, **Author** and **Contributor** roles. |
| **Feature** | **Admin** (`OrchardCore.Admin`) |

!!! tip "Your screens may look different"
    You see only the menus, buttons and lists that your role allows, and only for the features that are turned on. If something on this page is missing for you, see [Roles and permissions](roles-and-permissions.md).

To open the admin, sign in (see [Signing in](signing-in.md)) and go to `/Admin` on your site, for example `https://www.example.com/Admin`. Your site may use a different address for the admin; ask your administrator.

## The dashboard

The first page of the admin is the dashboard. Click the site name at the top left of any admin page to come back to it. The breadcrumb at the top of every page also starts with **Dashboard**.

On a site without the **Admin Dashboard** feature, the dashboard shows a **Welcome to Orchard Core** message and a few shortcut cards:

| Card | Button | Where it goes |
| --- | --- | --- |
| **Create content** | **Go to content** | The content list, see [Content items](../content/content-items.md). |
| **Customize theme** | **Manage themes** | The themes page, see [Themes](../design/themes.md). |
| **Manage users** | **Go to users** | The user list, see [Users](../users/users.md). |
| **Learn more** | **Documentation** | The Orchard Core documentation website. |

With the **Admin Dashboard** feature (`OrchardCore.AdminDashboard`) on, the dashboard shows widgets instead, and a **Manage Dashboard** button lets you arrange them. See [Dashboard](../design/dashboard.md).

## The admin menu

The menu on the left lists every area of the admin you can open. The items are grouped in sections, such as:

| Section | What you find there |
| --- | --- |
| **Content** | **Content Items** (all content) and one entry per content type, such as **Article** or **Page**. |
| **Design** | Content types, themes, widgets, templates, placements, shortcodes and workflows. |
| **Search** | Search indexes and queries. |
| **Access Control** | **Users**, **Roles** and sign-in applications. |
| **Media** | The media **Library** and media profiles. |
| **Multi-Tenancy** | **Tenants** and feature profiles. |
| **Tools** | Features, recipes, deployments, audit trail, background tasks and other tools. |
| **Settings** | Site-wide settings, from **General** to email, security and search. |

Your site may also show links of its own at the top of the menu, for example **Blog** or **Main Menu**. Administrators add them with custom admin menus, see [Admin menu](../design/admin-menu.md).

To use the menu:

1. Click a section name, such as **Content**, to open it.
2. Click an item to open its page.

The section that holds the page you're on is always open. Whether other sections close when you open one depends on the **Navigation menu behavior** setting, see [Change how the admin works](#change-how-the-admin-works-administrators).

### Make the menu smaller or larger

To shrink the menu:

1. Click **Collapse / expand menu** at the bottom of the menu.
2. Hover an icon to see its name.
3. Click a section icon to open its items in a small pop-out list.
4. Click **Collapse / expand menu** again to show the full menu.

The shrunk menu is a narrow bar of icons. On a narrow screen, such as a phone, the menu is hidden:

1. Click the menu button (three lines) at the top left to show it.
2. Click outside the menu, or press **Esc**, to hide it.

The admin remembers in your browser whether you shrank the menu and, with the **Persistent** menu behavior, which sections you opened, so it looks the same the next time you sign in on that browser.

### Filter the menu

When an administrator turns on **Enable Admin Menu filter**, a **Filter** box appears above the menu. Type part of a name, such as `users`, and the menu shows only the matching items. Press **Ctrl+Shift+F** to jump to the box, and **Esc** to clear it.

## The top bar

The bar across the top of every admin page holds, from left to right:

| Item | What it does |
| --- | --- |
| Site name and logo | Goes back to the dashboard. |
| Page title | Shown here only when **Display titles in top bar** is on. Otherwise the title is above the page content, under the breadcrumb. |
| **Quick navigation** (magnifying glass) | Opens a search box that jumps to any admin page, see [Jump to a page with quick navigation](#jump-to-a-page-with-quick-navigation). |
| Language picker | Shown when the site has more than one culture and the **Admin Culture Picker** feature is on. Changes the language of the admin screens. |
| **Notifications** (bell) | Shown when the **Notifications** feature is on. A number shows how many notifications you haven't read. Click it to see the latest ones, or click **Notification Center** to open them all. See [Notifications](../forms-and-automation/notifications.md). |
| **Toggle theme** (half circle) | Switches the admin between **Light**, **Dark** and **Auto** (follows your computer's setting). Your choice is kept in your browser. |
| **Visit Site** (arrow out of a box) | Opens the public site. |
| User menu (person icon) | Shows **Signed in as** and your user name, and links to **Profile**, **Change password**, **Security** and **Log off**. See [Signing in](signing-in.md). |

The user menu shows **Profile** only when you may edit your own account or view users. **Change password** is hidden when the site turns off password sign-in. **Security** appears when two-factor authentication is turned on for the site, and opens your two-factor settings. The menu can also show **External Logins**, when the site offers sign-in with an outside account, and **Change Email**, when the site lets users change their email address.

## Jump to a page with quick navigation

Quick navigation finds any admin page by name, so you don't have to look for it in the menu.

1. Click the magnifying glass in the top bar, or press **Ctrl+K** (**Cmd+K** on a Mac).
2. Type part of the page name in **Go to an admin page...**, for example `email`.
3. Use the **Up** and **Down** arrow keys to highlight a result.
4. Press **Enter**.

You can also click a result instead of using the arrow keys.

Each result shows where it sits in the menu, for example *Settings > Communication*. Press **Esc** to close the box. The **Ctrl+K** keys do nothing while you type in a rich text or code editor, because those editors use the keys themselves.

With the **Content Quick Navigation** feature (`OrchardCore.Contents.QuickNavigation`) on, the results also include the content items updated most recently, marked *Content*, so you can open a page or post to edit it straight away. You only see items you are allowed to edit and that have a title. Quick navigation searches page names and content titles, not the text inside your content.

## Breadcrumbs and page titles

Most admin pages show a breadcrumb above the title, for example **Dashboard / Manage Content / Article**. Click any part of it to go back to that page. Administrators can hide the breadcrumb with **Show breadcrumb**.

## Work with admin lists

Many admin pages show a list: content items, users, roles, queries, tenants and more. They all work the same way.

### Search and filter

1. Type in the **Search** box at the top of the list.
2. Press **Enter**, or click the magnifying glass next to the box.

Many lists also have a funnel button before the box, with ready-made filters and a **Filter syntax** link that explains the words you can type, and drop-down menus above the rows, such as **Show** or **Sort**. Each list's page explains its own filters. For the content list, see [Content items](../content/content-items.md#find-content).

The bar with the search box stays at the top of the screen while you scroll.

### Select items and use bulk actions

1. Tick the box at the start of each row you want, or tick the box above the rows to select every item on the page.
2. Click **Actions** above the rows.
3. Choose an action, such as **Publish Now** or **Delete**.
4. Click **Ok** in the **Bulk Action** confirmation box.

**Actions** appears once two or more items are selected, in place of the list's filter menus. The label above the rows shows how many items are on the page and how many there are in total, for example *10 items / 42 items in total*. While items are selected, it shows how many you selected instead.

!!! note
    Bulk actions work on two or more items. To act on a single item, use the buttons or the **Actions** menu of its row.

### Use the actions of a row

Each row has its own buttons, such as **Edit** or **View**. Less common actions are in the **Actions** menu at the end of the row. When an administrator chooses the **Menu** actions layout, all of a row's actions are in one menu, opened with the three-dots button.

### Switch between the List and Grid layouts

A list can show its items as a **List** (one card per row) or as a **Grid** (a table with column headings, such as **Title**, **Type**, **Status** and **Last modified** for content).

- Click the **List** or **Grid** button at the top right of the list.

Your choice is kept in your browser for that list only, so each list can stay the way you like it. The buttons appear only when **Let users choose the layout of a list** is on.

### Move between pages

Below the list, use the page numbers, or the buttons to go to the first, previous, next or last page. A list shows 10 items per page unless your administrator changes the **Page size** in **Settings > General**. When the administrator turns on **Allow users to change the page size on listing pages**, an **Items per page** menu appears next to the page numbers. See [General settings](../settings/general.md).

When nothing matches, the list says *No results found.*

## Read the messages after you save

After you save, publish or delete something, a message appears in the top right corner of the screen, for example *Your Article has been published.*

| Color | Meaning | How long it stays |
| --- | --- | --- |
| Green | It worked. | Disappears by itself after a few seconds. |
| Blue | Information, for example *Successfully cloned. The clone was saved as a draft.* | Until you close it. |
| Yellow | A warning: something wasn't done, for example *Couldn't publish selected content.* | Until you close it. |
| Red | An error. | Until you close it. |

Click the **x** on a message to close it.

When a form has a problem, nothing is saved. The page stays open, a list of the problems appears at the top of the form, and each wrong field shows its own message in red, for example *A value is required for Title.* Fix the fields and save again. Sections of the form that hold a wrong field open by themselves so you can see it.

When you leave a page by clicking **Delete**, **Discard Draft** or another action that can't be undone, a confirmation box asks you first.

## Keyboard shortcuts

| Keys | What they do |
| --- | --- |
| **Ctrl+K** (**Cmd+K** on a Mac) | Open quick navigation. |
| **Up**, **Down**, **Enter** | Move through the quick navigation results and open one. |
| **Ctrl+Shift+F** | Jump to the admin menu **Filter** box, when it is shown. |
| **Esc** | Close quick navigation, clear the menu filter, or hide the menu on a narrow screen. |

## Change how the admin works (administrators)

Administrators decide which of these tools everyone sees.

| | |
| --- | --- |
| **Menu** | **Settings > Admin** |
| **Permission** | *Manage Admin Settings* (given to the **Administrator** role by default) |
| **Feature** | **Admin** (`OrchardCore.Admin`) |

1. Open **Settings > Admin**.
2. Change the options in the table.
3. Click **Save**.

| Field | What it does |
| --- | --- |
| **Enable theme toggler** | Shows the **Toggle theme** button in the top bar. On by default. When it's off, the admin always uses the light look. |
| **Enable Admin Menu filter** | Shows the **Filter** box above the admin menu. Off by default. |
| **Enable quick navigation** | Shows the quick navigation button and turns on **Ctrl+K**. On by default. |
| **Display New menu** | Adds a **New** section to the admin menu, with one link per content type you can create, so you can start a content item from anywhere. Off by default. |
| **Display titles in top bar** | Shows each page's title in the top bar instead of above the content. Off by default. |
| **Show breadcrumb** | Shows the breadcrumb on admin pages. On by default. |
| **Navigation menu behavior** | **Focused** (the default): only the section of the current page is open, and opening a section closes the others. **Persistent**: sections stay open until you close them, so several can be open at once. |
| **List layout** | The layout every list starts with: **List** or **Grid**. **Use configuration default** keeps the layout your IT team set up, which is **List** unless they changed it. |
| **Actions layout** | How the actions of each row, and the less common buttons at the bottom of edit pages, are shown: **Buttons**, or a single compact **Menu**. **Use configuration default** keeps the setting your IT team chose. On small screens the less common buttons of edit pages are always in a menu. |
| **Let users choose the layout of a list** | Shows the **List** / **Grid** buttons on each list. On by default. Each person's choice applies to them only. |
| **Recent content items in quick navigation** | Shown with the **Content Quick Navigation** feature. How many of the most recently updated content items quick navigation includes. Must be at least 1; the default is 50. |
