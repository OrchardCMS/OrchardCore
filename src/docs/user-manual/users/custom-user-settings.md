---
title: Custom user settings
description: Add your own fields to every user account, such as a job title or a newsletter preference, and let people fill them in on their profile.
technical_manual:
  - reference/modules/Users/CustomUserSettings/README.md
  - reference/modules/Users/README.md
---

# Custom user settings

Every account has a user name, an email and a phone number. When you need more, such as a job title, a department, a short biography or a newsletter preference, custom user settings add your own fields to every user. You build them like a content type, with fields and parts, and they appear as an extra tab on the user editor and on each person's profile.

| | |
| --- | --- |
| **Menu** | **Design > Content Definition > Content Types** (to create the settings), **Access Control > Users** and **Profile** (to fill them in) |
| **Permission** | *Manage Own Custom User Settings - [type name]* to see and edit the tab. Building the type needs the permission to edit content types. |
| **Feature** | **Custom User Settings** (`OrchardCore.Users.CustomUserSettings`), which needs **Contents** |

## Create a set of custom user settings

Each set of settings is a content type with the **Custom User Settings** stereotype. Each set becomes one tab.

1. Turn on **Custom User Settings** in **Tools > Features** (see [Features](../getting-started/features.md)).
2. Open **Design > Content Definition > Content Types** and click **Create new type**.
3. Type a **Display Name**, for example *Staff details*, check the **Technical Name**, and click **Create**. The **Add Parts** page opens; click **Cancel** to go to the content type editor.
4. Untick **Creatable**, **Listable**, **Draftable** and **Securable**: these settings are not content items people create, list or publish on their own. Open the arrow next to **Save** and choose **and continue**. Do this before the next step: once the stereotype is saved, **Creatable**, **Listable** and **Draftable** are hidden and keep their values.
5. Type `CustomUserSettings` in **Stereotype**. You can also pick **Custom User Settings** from the suggestions the box offers. Choose **and continue** again.
6. Add the fields you need with **Add Field**, for example a text field named *Job title* and a boolean (yes/no) field named *Newsletter*.
7. Click **Save**.

For the content type editor, the fields and their settings, see [Content types](../content/content-types.md) and [Fields](../content/fields.md).

!!! warning
    Only use a new content type for custom user settings. Giving the stereotype to a content type that already has content items breaks those items.

You can create several sets, for example *Staff details* and *Preferences*. Each one gets its own tab and its own permission.

## Let people see and edit the settings

Each set has its own permission, *Manage Own Custom User Settings - Staff details* (with the display name of your type), in the **Custom User Settings** group of the role editor. Nobody has it by default except through *Manage security settings and all users*, which administrators have.

1. Open **Access Control > Roles** and edit the role, for example *Editor*.
2. Tick *Manage Own Custom User Settings - Staff details*.
3. Click **Save**.

See [Roles](roles.md).

The permission is checked for the person looking at the page. So:

- People whose role has the permission see the tab on their own **Profile** and can fill it in.
- An administrator with *Manage security settings and all users* sees the tab on every account in **Access Control > Users**.
- Someone who may edit users but doesn't have this permission doesn't see the tab, even on other people's accounts.

## Fill in the settings

1. Open your user menu and click **Profile**, or open **Access Control > Users** and click **Edit** on a person (or **Add User** for a new one).
2. Click the tab named after the settings, for example **Staff details**.
3. Fill in the fields.
4. Click **Save**.

The values are saved with the user account right away; there is no draft or publish step.

## Use the settings elsewhere

- Developers and template designers can show the values on the site, for example a job title next to an author's name. The Technical Manual explains how.
- In **Tools > Deployments**, the **Custom User Settings** step exports all sets or the ones you choose, so you can copy them to another site. See [Import and export](../administration/import-and-export.md).
- With the **Users Audit Trail** feature, each set can be kept in the snapshot of user events, see [Users](users.md#record-what-happens-to-user-accounts).

## Remove a set

To stop using a set, delete its content type in **Design > Content Definition > Content Types**. The tab disappears from the user editor and the profile.
