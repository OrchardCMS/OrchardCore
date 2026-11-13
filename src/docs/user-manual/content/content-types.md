---
title: Content types
description: Create and change content types, add parts and fields to them, set their options, and build reusable parts.
technical_manual:
  - reference/modules/ContentTypes/README.md
  - reference/modules/ContentParts/README.md
  - reference/modules/ContentFields/README.md
  - reference/modules/Contents/README.md
---

# Content types

A **content type** is the template for a kind of content: *Article*, *Blog Post*, *Page*, *Product*. It decides which boxes editors fill in when they create a content item of that type. A content type is built from **parts** (ready-made blocks of behavior, such as a title, a web address or a body text) and **fields** (single values you add yourself, such as a subtitle, a picture or a date). You can build new content types and change existing ones entirely in the admin, without a developer.

| | |
| --- | --- |
| **Menu** | **Design > Content Definition > Content Types**, **Design > Content Definition > Content Parts** |
| **Permission** | *View content types.* (see the lists), *Edit content types.* (create, change and delete) |
| **Feature** | **Content Types** (`OrchardCore.ContentTypes`) |


!!! warning "Changes apply to every content item of the type"
    Changing a content type changes the editor and the display of all its content items at once. Try big changes on a test site first.

## Find a content type

Open **Design > Content Definition > Content Types**. Each row shows the type's display name, its technical name and, when it has one, its stereotype and description. Type in the **Search** box to narrow the list as you type.

Each row has these buttons:

- **Create New ...**: creates a content item of that type. Shown only when the type is **Creatable**.
- **List Items**: opens the content items of that type.
- **Edit**: opens the content type editor.
- **Delete**: deletes the type. The button is disabled for system-defined types, which the site needs to work.

## Create a content type

1. Open **Design > Content Definition > Content Types** and click **Create new type**.
2. Type the **Display Name**, for example *Event*. The **Technical Name** fills in by itself as you type.
3. Change the **Technical Name** if you want, then click **Create**.
4. The **Add Parts** screen opens. Tick the parts the type needs (see [Add parts](#add-parts-to-a-content-type)) and click **Add Selected Parts**, or click **Cancel** to go straight to the type editor.

| Field | What it does |
| --- | --- |
| **Display Name** | Required. The name shown on screens, for example in the **New** menu. It must be different from the display name of every other type. |
| **Technical Name** | Required. The name the site uses internally, in templates and in web addresses of the admin. It must start with a letter, contain only letters and digits (no spaces, accents or punctuation), be unique, and not be a reserved name. You can't change it later. |

A new type is **Creatable**, **Listable**, **Draftable**, **Versionable** and **Securable**. It already has one part, named after the type, that holds the fields you add to the type.

## Change a content type's settings

Open **Design > Content Definition > Content Types** and click **Edit** on the type. The top of the editor holds the type's settings. Click **Save** at the bottom to apply them, or open the arrow next to **Save** and choose **and continue** to save and stay on the page.

| Setting | What it does |
| --- | --- |
| **Technical Name** | Read only. |
| **Display Name** | Required. The name shown on screens. |
| **Description** | Optional. A short explanation shown under the type in the content type list and in pickers. |
| **Creatable** | Editors can create content items of this type from the admin: the type appears in the **New** menu and has a **Create** button. Untick it for types that are only created inside another item, such as blog posts inside a blog. |
| **Listable** | Content items of this type appear in **Content > Content Items** and in its content type filter. |
| **Draftable** | Editors can save a draft without publishing. When it is off, the editor has no **Save Draft** button. |
| **Versionable** | The site keeps the earlier versions of a content item when it is published again. The [audit trail](audit-trail.md) uses these versions to compare and restore. |
| **Securable** | The type gets its own permissions in the role editor (for example *Publish or unpublish Event for others*), so you can let a role edit one type but not another. See [Roles](../users/roles.md). |
| **Stereotype** | Optional. Gives the type a special role. Pick one from the list or type it: **Widget** (a block placed in zones or inside a [flow](flows-and-bags.md)), **Menu Item**, **Custom Settings**, **Custom User Settings**, **DashboardWidget**. Leave it empty for ordinary content. It must start with a letter and contain only letters and digits. For some stereotypes, such as **Custom Settings**, some of the boxes above are hidden after you save. |
| **Category** | Optional. Groups types in content type pickers, for example *Layout* or *Media*. |
| **Thumbnail** | Optional. The path of a small picture shown for the type in content type pickers. |
| **Full-text** | Decides which text of the content items is used for free-text searches: the display text, the body parts, and an optional Liquid template. See [Site search](../search/site-search.md). |

!!! note
    When the **Templates** or **Placements** features are on, the editor also shows **Edit templates** and **Edit placements** buttons. They open the matching designer screens, see [Templates and placements](../design/templates-and-placements.md).

## Add parts to a content type

1. Open the type editor and click **Add Parts** (under the list of parts).
2. Tick each part you want and click **Add Selected Parts**. To add one part and set it up straight away, click **Add & Configure** on its row instead.

The list shows only the parts the type doesn't have yet and that can be attached to types.

### Add a named part

Some parts can be added more than once to the same type, each copy with its own name, for example two **Bag** parts called *Slides* and *Team*. These are **named parts**.

1. In the type editor, click **Add Named Part**.
2. Type the **Display Name** and the **Technical Name** (it fills in by itself), and optionally a **Description**.
3. Choose the part to add from the list.
4. Click **Save**, or **Save & Configure** to open its settings.

Only parts marked **Reusable** are offered. When there are none, the screen says *There are no available named parts.*

## Add fields to a content type

1. Open the type editor and click **Add Field** (under the list of fields).
2. Type the **Display Name**, for example *Start date*. The **Technical Name** fills in by itself.
3. Pick the **Field Type**, for example **Date Time Field**.
4. Click **Save** to go back to the type, or **Save & Configure** to open the field's settings.

| Field | What it does |
| --- | --- |
| **Display Name** | Required. The label editors see. It must be different from the other fields of the type. |
| **Technical Name** | Required. Must start with a letter, contain only letters and digits, and be unique in the type. You can't change it later. |
| **Field Type** | Required. The kind of value: text, number, date, media, link, content picker, and so on. The list depends on the features that are on. See [Fields](fields.md) for every field type. |

## Change a field's settings

1. In the type editor, click **Edit** on the field's row.
2. Change the **Display Name** if needed. The **Technical Name** is read only.
3. Set the options of the field type, such as **Required**, **Hint**, **Placeholder** or **Default value**. Each field type has its own options, described on the [Fields](fields.md) page.
4. When the field type has several editors, pick one in **What type of editor should be used?** (for example **Standard**, **Multi-line**, **Email** or **Url** for a text field). Pick how it is shown on the site in **What type of display mode should be used?**.
5. Click **Save**.

The field settings may also show index options (**Include this element in the index**, ...), used by [site search](../search/site-search.md).

## Change a part's settings on a type

1. In the type editor, click **Edit** on the part's row.
2. Change the options of the part, then click **Save**.

The options depend on the part: for example the **Title** part has a **Behavior option**, **Render Title**, a **Pattern** and a **Placeholder**; the **List** part has a **Page Size** and **Contained Content Types**. The page of each part, listed in [Built-in parts](#built-in-parts), describes its settings. For a named part, you can also change its **Display Name** and **Description** here. Some parts also offer **What type of editor should be used?** and **What type of display mode should be used?**.

The part settings may also show **GraphQL** options (**Collapse**, **Prevent Field Name Collision**, **Hide**), which are for developers who read content through the GraphQL API, and index options used by site search.

The green banner at the top links to the **Part Definition**: settings there apply to the part on every type (see [Create a reusable part](#create-a-reusable-part)).

## Reorder parts and fields

The order of parts and fields in the type editor is the order editors see them in the content item editor.

1. In the type editor, drag a part or a field up or down its list.
2. Click **Save**.

The row named after the type, marked *Contains the fields for the current type*, holds the type's fields. Drag it to choose where those fields appear among the parts.

## Remove a part or a field

1. In the type editor, click **Remove** on the part's or field's row.
2. Confirm.

The **Remove** button is disabled for system-defined parts and fields, which the type needs to work.

## Delete a content type

1. Open **Design > Content Definition > Content Types** and click **Delete** on the type's row, or click **Delete** at the bottom of the type editor.
2. Confirm with **Ok**.

The type and the part that held its fields are removed. The content items created with the type are not deleted, but they no longer show in the admin lists. Delete or move those content items first if you don't need them. System-defined types can't be deleted.

## Create a reusable part

A part you build yourself is a group of fields you can add to several content types, for example an *Address* part with street, city and postal code fields.

1. Open **Design > Content Definition > Content Parts** and click **Create new part**.
2. Type the **Name**, without spaces or special characters, for example *Address*, and click **Create**.
3. The part editor opens. Fill in the settings below, add fields with **Add Field**, and click **Save**.
4. Add the part to content types with **Add Parts** (or **Add Named Part** if you ticked **Reusable**).

| Setting | What it does |
| --- | --- |
| **Technical Name** | Read only. |
| **Display Name** | The name shown on screens. Leave it empty to use the technical name. |
| **Description** | Optional. Shown next to the part in the **Add Parts** list. |
| **Attachable** | The part can be added to content types. Tick it, or the part won't be offered in **Add Parts**. |
| **Reusable** | The part can be added more than once to the same type, as a named part. |

The **Content Parts** list shows every part that has stored settings, including built-in ones. Its **Edit** button opens the same part editor, where settings apply to the part on all types. Its **Delete** button deletes the part definition after you confirm: use it only for parts you created and no longer use.

## Built-in parts

Which parts are offered depends on the features that are on. The most common ones:

| Part | What it adds | More |
| --- | --- | --- |
| **Title** | A title for the content item, typed or generated from a pattern. | [URLs and homepage](urls-and-homepage.md) |
| **Autoroute** | A web address (permalink), typed or generated, and the option to make the item the homepage. | [URLs and homepage](urls-and-homepage.md) |
| **Alias** | A short, stable name to find the item from templates. | [URLs and homepage](urls-and-homepage.md) |
| **HtmlBody** | A rich text (HTML) body. | [Editing text](editing-text.md) |
| **MarkdownBody** | A Markdown body. | [Editing text](editing-text.md) |
| **Liquid** | A body written in Liquid, for site builders. | [Editing text](editing-text.md) |
| **List** | Turns the item into a container of other items, such as a blog of posts. | [Lists and blogs](lists-and-blogs.md) |
| **Flow** | A body built from widgets you stack and size. | [Flows and bags](flows-and-bags.md) |
| **Bag** | A collection of other content items edited inside the item. | [Flows and bags](flows-and-bags.md) |
| **WidgetsList** | Widgets placed in the zones of the theme for this item only. | [Widgets and layers](../design/widgets-and-layers.md) |
| **Localization** | A culture and translations of the item. | [Localization](localization.md) |
| **PublishLater** | A date and time to publish the item. | [Scheduling](scheduling.md) |
| **ArchiveLater** | A date and time to unpublish the item. | [Scheduling](scheduling.md) |
| **SeoMeta** | Page title, meta description, social sharing tags. | [SEO](seo.md) |
| **Preview** | The address used to preview the item when the site runs separately from the admin. | [Preview](preview.md) |
| **AuditTrail** | A **Comment** box saved with the change in the audit trail. | [Audit trail](audit-trail.md) |
| **Common** | Editors for the creation date and the owner of the item (see below). | |
| **Sitemap** | Lets editors exclude the item from the sitemap or set its priority. | [Sitemaps](../search/sitemaps.md) |
| **Form** and the form element parts | Turn items into forms and form elements. | [Forms](../forms-and-automation/forms.md) |
| **LinkMenuItem**, **Content Menu Item**, **Html Menu Item**, **MenuItemPermission** | Menu items and their options. | [Menus](../design/menus.md) |
| **FacebookPlugin** | Meta (Facebook) social plugin widgets. | [Analytics and social](../settings/analytics-and-social.md) |
| **Search Form Part** | A site search box. | [Site search](../search/site-search.md) |

Taxonomy terms are chosen with the **Taxonomy Field**, not a part. See [Taxonomies](taxonomies.md).

### Common part settings

| Setting | What it does |
| --- | --- |
| **Display date editor** | Editors can change the creation date of the content item. |
| **Display owner editor** | Editors can change the owner of the content item. They also need the *Edit the owner of a content item* permission. |
