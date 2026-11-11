---
title: Roles and permissions
description: Why two people see different menus, what the built-in roles can do, and how to ask your administrator for access.
technical_manual:
  - reference/modules/Roles/README.md
  - reference/modules/Security/Permissions.md
  - topics/security/roles.md
---

# Roles and permissions

Two people who sign in to the same site often see different admin menus. That is on purpose. What you can see and do depends on two things: which **features** are turned on for the site, and which **permissions** your **roles** give you. This page explains how that works, what the roles that come with Orchard Core can do, and how to ask for more access.

| | |
| --- | --- |
| **Menu** | **Access Control > Roles** (to look at roles), **Access Control > Users** (to give roles to people) |
| **Permission** | *View Roles* to look at roles, *Manage Roles* to change them |
| **Feature** | **Roles** (`OrchardCore.Roles`) |

## How access works

- A **permission** allows one thing, for example *Publish or unpublish content for others* or *Manage Media*. Each feature brings its own permissions.
- A **role** is a named set of permissions, such as *Editor*. An administrator ticks the permissions each role has.
- A **user** has any number of roles. You get every permission of every role you have, added together. No role takes a permission away.
- On top of your own roles, everyone who is signed in also gets the permissions of the **Authenticated** role, and everyone, signed in or not, gets those of the **Anonymous** role.

A menu item appears only when its feature is on **and** you have the permission it needs. Pages of this manual list both in the table at the top.

Some permissions include others. For example, *Manage security settings and all users* includes *Edit any user*, *Delete any user* and *List all users*. In the role editor, the **Effective** column shows everything a role really gets, including what is included this way.

## The built-in roles

Every site has three **system roles**, which can't be deleted. The starter recipes (Blog, Agency, Coming Soon, Blank, Headless) also create four roles for common jobs. The list below shows what each role can do on a fresh site. The exact permissions depend on which features are on, because each feature adds its own defaults to these roles when it is turned on for the first time. An administrator can change any of them.

| Role | Kind | What it can do by default |
| --- | --- | --- |
| **Administrator** | System | Everything. The role always has every permission, and its permissions can't be edited. |
| **Authenticated** | System | Given to everyone who is signed in, without being assigned. By default: view content. With the matching features on, also: manage their own notifications (**Notifications**), view their own media (**Secure Media**) and see audit trail events about their own account (**Users Audit Trail**). The Headless recipe also lets it run GraphQL and run every query through the API. |
| **Anonymous** | System | Applies to every visitor, including people who aren't signed in. By default: view published content. |
| **Editor** | Starter | Opens the admin and manages content written by anyone: edit, publish, unpublish, delete, clone and preview. Also manages media, menus, templates, placements, shortcode templates, queries, Liquid templates and the admin menu, translates content and runs workflows. |
| **Author** | Starter | Opens the admin and creates content: edits, publishes, unpublishes, deletes, clones and previews **their own** content items, and manages their own media. |
| **Contributor** | Starter | Opens the admin and writes content but can't publish: edits, clones and previews **their own** content items, and manages their own media. An Editor publishes their work. |
| **Moderator** | Starter | Opens the admin and the dashboard. By default it has no content permissions of its own beyond what every signed-in user has; the administrator adds what moderating means on your site. |

Editor, Author, Contributor and Moderator can also edit their own profile and, with the **Notifications** feature, manage their own notifications. With **Content Localization** on, Authors and Contributors can also translate their own content items.

!!! note "Authors and Contributors don't see Content > Content Items"
    By default, only **Administrator** and **Editor** have *List content items*, so the content lists are hidden from Authors and Contributors. Before they start, an administrator adds *List content items* to their role, or the per-type permission such as *List Article content items* to show only some types. See [Roles](../users/roles.md).

!!! note
    The descriptions the starter roles carry (for example *Grants users the ability to moderate content.*) are only labels. What a role can really do is the ticked permissions. To check, open **Access Control > Roles** and the role.

## Why a menu or button is missing

When you can't find something:

1. Look up the task in this manual.
2. Read the **Menu**, the **Permission** and the **Feature** in the table at the top of the page.
3. Check whether the menu is missing for everybody, or only for you.

If the menu is missing for everybody, the feature is probably off. An administrator turns it on in **Tools > Features** (see [Features](features.md)). If a colleague sees it and you don't, your roles lack the permission.

A few common examples:

| You can't... | Usually because |
| --- | --- |
| Open the admin at all | None of your roles has *Access admin panel*. |
| Publish, only save drafts | You have *Edit own content* but not *Publish or unpublish own content* (the Contributor role works this way). |
| Edit someone else's article | You have the "own" permissions only, not *Edit content for others*. |
| See **Access Control > Users** | You don't have *List all users* or one of the "users in a role" permissions. |
| See a **Settings** page | Each settings page has its own permission, often *Manage security settings and all users* or a *Manage ... settings* permission. |

## Ask your administrator for access

Send your administrator a message like this one, filled in with what you found in this manual:

> Hi,
>
> I need access to **[screen or task, for example: Content > Content Items, publishing articles]** for my work on **[what you need to do]**.
>
> According to the User Manual, this needs:
>
> - Permission: **[permission name from the page]**
> - Feature: **[feature name from the page]**
>
> I currently have the **[your role or roles]** role. Could you add the permission to one of my roles, or give me a role that has it? If the feature is turned off on our site, could you let me know whether we plan to use it?
>
> Thank you,
> [Your name]

You find your roles on your **Profile** page when you can assign roles, or by asking the administrator.

## For administrators

- To give roles to people, see [Users](../users/users.md).
- To create roles and choose their permissions, see [Roles](../users/roles.md).
- To turn features on, see [Features](features.md).
- For a worked example of a limited editor account, see [An editor with limited access](../use-cases/editor-with-limited-access.md).
