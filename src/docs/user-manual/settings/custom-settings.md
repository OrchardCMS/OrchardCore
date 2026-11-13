---
title: Custom settings
description: Create your own settings screens, such as a footer text or a company address, from a content type with the Custom Settings stereotype.
technical_manual:
  - reference/modules/CustomSettings/README.md
  - reference/modules/Settings/README.md
---

# Custom settings

Custom settings let you add your own screens under **Settings**, for values that are the same across the whole site: a footer text, a phone number, a company address, a promotional banner, social media links. You design the screen yourself, with the same [parts and fields](../content/content-types.md) you use for content types, and your theme shows the values on the site.

Each custom settings screen is a content type with the **Custom Settings** stereotype. It has exactly one set of values for the site, so there is no list of items: you open the screen, fill it in, and save.

| | |
| --- | --- |
| **Menu** | **Settings > (the name of your settings type)**; you create the type in **Design > Content Definition > Content Types** |
| **Permission** | *Manage Custom Settings - (name of the type)*, one for each custom settings type. To create the type you need the permissions of [content types](../content/content-types.md). |
| **Feature** | **Custom Settings** (`OrchardCore.CustomSettings`) |


## Create a custom settings screen

1. Open **Design > Content Definition > Content Types** and click **Create new type**.
2. Type a **Display Name**, for example *Footer Settings*. The **Technical Name** is filled in for you (*FooterSettings*). Click **Create**.
3. On the edit screen of the type, find the **Stereotype** box and type `CustomSettings`, or pick **Custom Settings** from the suggestions.
4. Ignore **Creatable**, **Listable**, **Draftable** and **Versionable**: they don't apply to settings, and they are hidden once the type is saved with this stereotype.
5. Click **Add Field** to add the fields you need, for example a **Text Field** named *Copyright* and a **Link Field** named *Privacy page*. You can also click **Add Parts** to add parts, such as **HtmlBody**.
6. Click **Save**.

Your screen appears in the admin menu under **Settings**, with the display name of the type, for example **Settings > Footer Settings**.

!!! warning "Use a new content type"
    Give the **Custom Settings** stereotype only to a content type you create for this purpose. Don't add it to a type that already has content items, such as **Page** or **Article**: those items would stop working.

## Fill in your custom settings

1. Open **Settings** and click your settings screen, for example **Settings > Footer Settings**.
2. Fill in the fields.
3. Click **Save**.

You see *Site settings updated successfully.* The values apply right away. There is no draft or publish step: what you save is live.

## Let a role edit a custom settings screen

Every custom settings type has its own permission, so you can let a team edit the footer without giving them other settings.

1. Open **Access Control > Roles** and click **Edit** next to the role.
2. Find the permission *Manage Custom Settings - Footer Settings* (the name ends with the display name of your type). It is listed under the **Custom Settings** feature.
3. Check it and click **Save**.

People with that role then see **Settings > Footer Settings**. See [Roles](../users/roles.md).

## Change or remove a custom settings screen

- To add, rename or remove fields, edit the content type in **Design > Content Definition > Content Types**. The screen under **Settings** follows the type.
- To remove the screen, delete the content type. The menu item and its permission disappear.

## Show the values on the site

A theme or a [template](../design/templates-and-placements.md) reads the values and shows them on the site, for example the footer text at the bottom of every page. Your developer or designer adds them to the theme and makes the settings available to templates; see the Technical Manual.

Custom settings are also included when you export site settings with a [deployment plan](../administration/import-and-export.md), through the **Custom Settings** step.
