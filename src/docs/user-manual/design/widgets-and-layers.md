---
title: Widgets and layers
description: Put widgets such as text, images and forms into the zones of your theme, and use layers and rules to decide on which pages and for whom they appear.
technical_manual:
  - reference/modules/Layers/README.md
  - reference/modules/Rules/README.md
  - reference/modules/Widgets/README.md
---

# Widgets and layers

A **widget** is a small block of content, such as a paragraph, an image, a raw HTML snippet or a form, that sits in an area of the page called a **zone** (for example the footer). A **layer** is a set of conditions, called **rules**, that decides when its widgets are shown: on every page, only on the home page, only for signed-in visitors, and so on. Use widgets and layers for things that appear on many pages, like a footer text or a sign-up form, without adding them to each content item.

| | |
| --- | --- |
| **Menu** | **Design > Widgets**; **Settings > Zones** |
| **Permission** | *Manage layers* |
| **Feature** | **Layers** (`OrchardCore.Layers`), which turns on **Widgets** (`OrchardCore.Widgets`) and **Rules** (`OrchardCore.Rules`) |


## How it fits together

1. **Settings > Zones** lists the zones you can put widgets in. The theme must show these zones, or their widgets never appear.
2. On **Design > Widgets**, you create layers and give each one rules.
3. You add a widget to a zone and choose its layer.
4. When a visitor opens a page, each layer's rules are checked. A widget is shown when its layer's rules are all true and the widget is published.

## Choose the zones

1. Open **Settings > Zones**.
2. In **Available zones for Layers**, type the zone names separated by commas, for example `Content, Footer`.
3. Click **Save**.

The names must match the zones of your site theme, letter for letter. The built-in site themes show these zones:

| Zone | Where it appears |
| --- | --- |
| `Header` | Under the navigation bar, above the page content. |
| `Content` | In the main column, with the page's own content. |
| `Footer` | At the bottom of every page. |
| `HeadMeta` | Inside the page's hidden header, for code such as tracking scripts. Nothing is visible there. |
| `Messages` | Above the content, where the site shows notifications. |

The Coming Soon theme shows all of them except `Messages`, and the Safe Mode theme shows only `Header`, `Content` and `Footer`. If you use another theme, ask its author which zones it shows. Until at least one zone is set, **Design > Widgets** asks you to *define the available zones* first.

## See the widgets and layers

Open **Design > Widgets**. The page is titled **Widgets and Layers** and has two columns:

- **Zones**, on the left: one box per zone, listing its widgets in order. Each widget shows its title and type, a badge with its layer, its status (such as **Published**) and its actions. Under the widgets, **Add Widget** lists the widget types you can add.
- **Layers**, on the right: each layer with its description and **Edit** and **Delete** buttons, and an **Add** button below. Tick a layer's box to highlight its widgets in green in the zones.

## Add a layer

1. On **Design > Widgets**, click **Add** under the layers.
2. Type a **Name**. It is required and must be unique. It can't be changed later.
3. Optionally type a **Description**, shown in the layer list.
4. Click **Create**.

The layer is created without rules. **A layer without rules never shows its widgets**, so add at least one rule next.

!!! tip
    The **Blog** and **Agency** recipes create two layers for you: **Always** (every page, with a rule that is always true) and **Homepage** (only the home page).

## Add rules to a layer

1. On **Design > Widgets**, click **Edit** on the layer. The page is titled **Edit Layer - *name***.
2. Next to **Rules**, click **Add**. The **Available Rules** window opens; type in **Filter** to find a rule.
3. Click **Add** on the rule you want.
4. Fill in the rule's fields (see below) and click **Create**.
5. Back on the layer, click **Save** if you changed the description.

Every rule at the top level must be true for the layer to show. To say "this **or** that", add an **Any condition** group and put the rules inside it with the group's own **Add** button. Each rule has **Edit** and **Delete** buttons, and you can drag rules to change their order.

### Rule types

| Rule | Fields | True when |
| --- | --- | --- |
| **All condition group** | **Name** (optional label) | Every rule inside it is true. An empty group is never true. |
| **Any condition** | **Name** (optional label) | At least one rule inside it is true. |
| **Boolean condition** | **True** or **False** | Always true, or always false. Use **True** for "every page", **False** to hide a layer for a while. |
| **Homepage condition** | **Is homepage** or **Is not homepage** | The visitor is, or isn't, on the home page. |
| **Url condition** | **Operation**, **Value** | The address of the page matches the value, such as `/blog` with **Starts with**. Start the value with `/` (or `~/`). |
| **Culture condition** | **Operation**, **Value** | The language of the page matches, such as `fr` or `fr-CA`. A value of `fr` also matches `fr-CA`. |
| **Role condition** | **Operation**, **Value** | The visitor has a role matching the value, such as `Editor`. |
| **Content type condition** | **Operation**, **Value** | A content item of a matching type is shown on the page, such as `BlogPost`. |
| **Is authenticated condition** | none | The visitor is signed in. |
| **Is anonymous condition** | none | The visitor isn't signed in. |
| **Javascript condition** | **Script** (required) | A short script returns true. For people comfortable with code. |

The **Operation** list offers **Equals**, **Does not equal**, **Starts with**, **Does not start with**, **Ends with**, **Does not end with**, **Contains** and **Does not contain**.

For the **Javascript condition**, the script can use `isHomepage()`, `isAuthenticated()`, `isAnonymous()`, `isInRole('Editor')`, `url('/blog*')` (a `*` at the end matches the start of the address) and `culture('fr')`, combined with `&&` (and), `||` (or) and `!` (not). For example, this shows a layer on the blog pages for visitors who aren't signed in:

```javascript
url('/blog*') && isAnonymous()
```

## Add a widget to a zone

1. On **Design > Widgets**, find the zone.
2. Click **Add Widget** in that zone and choose a widget type.
3. Fill in the widget editor:

    | Field | What it does |
    | --- | --- |
    | **Title** | Required. The name of the widget in the admin list. |
    | **Render title** | Ticked: the title is also shown above the widget on the site. |
    | **The layer** | Required. The layer that controls when the widget is shown. |
    | Other fields | Depend on the widget type, such as the text of a **Paragraph** or the image of an **Image** widget. |

4. Click **Publish** to show it on the site, or **Save Draft** to keep working on it.

The widget is added at the end of the zone. You need the content permissions for that widget type, such as creating and publishing it, as for any content item.

## Edit, unpublish or delete a widget

Each widget in a zone has the usual content item actions: **Edit**, **View**, and in the **Actions** menu **Unpublish**, **Delete**, **Clone** and others. They work as for any content item; see [Content items](../content/content-items.md). An unpublished widget stays in its zone but isn't shown on the site.

## Move and reorder widgets

Drag a widget up or down within its zone to change its order, or drag it into another zone to move it there. The page saves the new position at once and shows *Widget successfully moved.* with an **Undo** link to put it back.

## Delete a layer

Click **Delete** on the layer and confirm. A layer that still holds widgets can't be deleted: the site says *The layer couldn't be deleted: you must remove any associated widgets first.* Move those widgets to another layer (edit each widget and change **The layer**) or delete them first.

## Widget types

Any content type with the **Widget** stereotype can be added as a widget. Which ones you have depends on your theme, recipe and features. A site set up with a starter recipe typically has:

- **Paragraph**, **Image**, **Blockquote** and **Raw Html**, from the theme's recipe.
- **Form** and the form elements such as **Input**, **Text Area**, **Select**, **Button**, **Label**, **Validation** and **Container**, from the **Forms** feature. See [Forms](../forms-and-automation/forms.md).
- **Search Form**, from the **Search** feature.
- **Re Captcha** and the **Facebook** plugin widgets, from their features.

To create your own widget type, create a content type and set its stereotype to `Widget`. See [Content types](../content/content-types.md).

## Put widgets inside a content item

The **WidgetsList** part lets a single content item, such as a landing page, carry its own widgets in its own zones, without layers. Add the part to a content type (see [Content types](../content/content-types.md)) and set **Available widgets zones** in the part's settings, a comma-separated list such as `Header, Footer`.

On the content item's editor, each zone is a box with a **+** button (**Add Widget**) that lists the widget types. Each added widget is a card you can expand or collapse, drag by its handle to reorder, insert another widget next to (**Insert Widget**), or remove with the trash button (*Are you sure you want to remove this item?*). The widgets are saved with the content item.

These widgets show only on that content item's page, and only if the theme shows those zones.

!!! example "See it in practice"
    [Build a landing page](../use-cases/landing-page-with-widgets.md) walks through this step by step.
