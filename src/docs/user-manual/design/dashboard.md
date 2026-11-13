---
title: Admin dashboard
description: Understand the welcome page of the admin, and build your own dashboard of widgets that you add, arrange, resize and remove.
technical_manual:
  - reference/modules/AdminDashboard/README.md
  - reference/modules/Admin/README.md
---

# Admin dashboard

The dashboard is the first page you see after signing in to the admin, and the page the **Dashboard** link at the top of the admin menu opens. Out of the box it is a welcome page with shortcuts. With the **Admin Dashboard** feature, it becomes a board of widgets, such as notes or links for your team, that you arrange yourself.

| | |
| --- | --- |
| **Menu** | **Dashboard** (top of the admin menu) |
| **Permission** | *Access the Admin Dashboard* to see the widgets; *Manage the Admin Dashboard* to change them |
| **Feature** | **Admin** (`OrchardCore.Admin`) for the welcome page; **Admin Dashboard** (`OrchardCore.AdminDashboard`) for widgets |

!!! tip "Don't see the widgets?"
    Administrators have both permissions by default; Editors, Moderators, Authors and Contributors can see the dashboard but not change it. See [Roles and permissions](../getting-started/roles-and-permissions.md).

## The welcome dashboard

Without the **Admin Dashboard** feature, the dashboard shows *Welcome to Orchard Core* and a row of cards. Each card links to a common task, and appears when its feature is on:

| Card | Link | Opens |
| --- | --- | --- |
| **Create content** | **Go to content** | The content items list. See [Content items](../content/content-items.md). |
| **Customize theme** | **Manage themes** | **Design > Themes**. See [Themes](themes.md). |
| **Manage users** | **Go to users** | The users list. See [Users](../users/users.md). |
| **Learn more** | **Documentation** | The Orchard Core documentation. |

With the **Admin Dashboard** feature on, people who can't manage the dashboard and have no widget to see get this same welcome page.

## Build a dashboard of widgets

With **Admin Dashboard** on, the dashboard shows the published dashboard widgets as tiles in a grid. If there are none yet, someone who can manage it sees *Click on Manage Dashboard button to add dashboard widget.*

### Add a widget

1. Open the dashboard and click **Manage Dashboard** (top right).
2. Click **Add Widget** and choose a widget type, such as **Html Dashboard Widget**.
3. Fill in the widget's fields. For the Html Dashboard Widget: a **Title** and the HTML body, plus the dashboard fields below.
4. Click **Publish**. You return to the dashboard editor.
5. Click **Close Editor** to see the dashboard as everyone else does.

| Field | What it does |
| --- | --- |
| **Position** | The order of the widget on the dashboard. Lower numbers come first. |
| **Width** | How many grid columns the widget spans. 1 by default. |
| **Height** | How many grid rows the widget spans. 1 by default. |

The grid has six columns on a large screen, three on a tablet and one on a phone.

### Arrange and resize widgets

In **Manage Dashboard**:

- Drag a widget by its handle to move it to another place.
- Drag the corner of a widget to make it wider or taller; it snaps to whole grid cells.

Each change is saved at once and the page shows *Dashboard successfully updated.* with an **Undo** link.

### Edit, unpublish or remove a widget

In **Manage Dashboard**, each widget has an **Actions** menu (the three dots at the top right of the widget) with these entries, depending on your permissions for that widget type:

| Entry | What it does |
| --- | --- |
| **Edit** | Opens the widget's editor. |
| **Delete** | Deletes the widget, after you confirm. |
| **Unpublish** | Hides a published widget from the dashboard without deleting it. |
| **Publish Draft** | Publishes the changes saved as a draft. |
| **Discard Draft** | Throws away the draft and keeps the published version. |

Only published widgets appear on the dashboard. **Manage Dashboard** shows the latest version of each widget, including drafts.

## Who sees which widget

- People with *Access the Admin Dashboard* see the published widgets that their roles are allowed to view. Use the view permissions of each widget type in [Roles](../users/roles.md) to show a widget to some roles only.
- People with *Manage the Admin Dashboard* see every widget, and the widget types they are allowed to edit in **Add Widget**.

## Create your own widget types

Any content type with the stereotype `DashboardWidget` can be added to the dashboard. When you set that stereotype on a content type, the **Dashboard** part with **Position**, **Width** and **Height** is added to it for you. Add the parts and fields you need, such as an HTML body, a list of links, or a query result. See [Content types](../content/content-types.md).

The feature comes with the **Html Dashboard Widget** type: a title and an HTML body.
