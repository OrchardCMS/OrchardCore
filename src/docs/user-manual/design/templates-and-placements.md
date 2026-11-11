---
title: Templates and placements
description: Change how pages and parts of pages look with Liquid templates written in the admin, and move or hide pieces of a page with placement rules.
technical_manual:
  - reference/modules/Templates/README.md
  - reference/modules/Placements/README.md
  - reference/modules/Placement/README.md
  - reference/modules/Liquid/README.md
  - topics/display/README.md
---

# Templates and placements

These screens are for people who want to change how the site looks without asking a developer to change the theme. A **template** replaces the HTML of one piece of a page, written in Liquid, a simple template language. A **placement** decides where the pieces of a content item go on the page, or hides them. Both are stored in the site's database and win over what the theme says.

If you have never written HTML, start small, keep a copy of what you change, and test on a copy of the site first.

| | |
| --- | --- |
| **Menu** | **Design > Templates**; **Design > Admin Templates**; **Design > Placements** |
| **Permission** | *Manage templates* and *Manage Liquid templates*; *Manage admin templates* and *Manage Liquid templates*; *Manage placements* |
| **Feature** | **Templates** (`OrchardCore.Templates`); **Admin Templates** (`OrchardCore.AdminTemplates`); **Placements** (`OrchardCore.Placements`). Templates need **Liquid** (`OrchardCore.Liquid`). |


!!! warning "These permissions are powerful"
    *Manage templates*, *Manage admin templates* and *Manage Liquid templates* are marked security critical: a template can show any data the site has to anyone who visits a page. Give them only to people you trust.

## Shapes: what a template replaces

Every piece of a page, such as a whole content item, its title, a field or a menu, is drawn from a **shape** with a name. A template takes the shape's name and replaces its HTML. Some common names:

| Template name | What it replaces |
| --- | --- |
| `Content__Article` | An Article shown on its own page. Replace `Article` with your content type's technical name. |
| `Content_Summary__Article` | An Article shown in a list, such as a blog's list of posts. |
| `Content__Alias__about` | The content item whose alias is `about`. |
| `Widget__Paragraph` | Every Paragraph widget. |

The full list, with the names used by fields, parts, menus and more, is in the Technical Manual.

The easiest way to get the right name: open **Design > Content Definition > Content Types**, edit a type, and use the **Edit templates** button. It lists *Template for a ... content item in detail views* and *Template for a ... content item in summary views*, and opens the template editor with the name filled in. The **Edit** page of each part of the type, opened from the content type editor, has the same button.

## Create a template

1. Open **Design > Templates** and click **Add Template**.
2. Type the shape **Name**, such as `Content__Article`. It is required and must be unique.
3. Optionally type a **Description** to remind you what the template is for.
4. Write the template in **Content**, the Liquid editor. It is required.
5. Click **Save**, or open the arrow next to it and click **and continue** to stay in the editor.

The template is used straight away, for every visitor.

Here is a small template for `Content__Article`. It shows the title in a heading, then everything the content item's parts and fields would normally show:

```liquid
<article class="my-article">
    <h1>{{ Model.ContentItem.DisplayText }}</h1>
    {{ Model.Content | shape_render }}
</article>
```

`{{ ... }}` prints a value. `Model.ContentItem` is the content item being shown, and `Model.Content` holds the shapes of its parts and fields. The Technical Manual describes every value and filter you can use.

### Preview a template while you type

On **Design > Templates**, while creating or editing a template, click **Preview** at the bottom of the form. A new tab, **Template Preview**, opens your site. As you type in the template editor, the preview tab refreshes with your unsaved template. Browse to a page that uses the template to see it. If the tabs lose contact, the preview shows *Preview Disconnected*; errors in the template show *Preview Error* with details.

### Edit or delete a template

Open **Design > Templates**. Each row has **Edit** and **Delete**. Type in **Search** and press Enter to filter by name. To delete several templates, tick them and choose **Delete** from the **Actions** menu.

Deleting a template puts back the theme's own design for that shape.

## Change the admin with admin templates

**Design > Admin Templates** works the same way, but its templates replace shapes in the admin instead of the public site, for example to change how a content type looks in the admin list. Click **Add Template**, type the **Name** and **Content**, and save. There is no preview for admin templates.

!!! warning
    A broken admin template can make parts of the admin hard to use. Keep the original text so you can put it back, and delete the admin template to return to normal.

## Place, move or hide parts of a page

A content item is made of parts and fields, and each one is drawn as a shape placed in a zone of the page (such as `Content` or `Meta`) at a position. **Placements** let you change that without a template, for example to hide the tags in blog post summaries, or to show a field before the body.

### Create a placement

1. Open **Design > Placements** and click **Add Placement**.
2. Type the **Shape type**, such as `TextField` or `TitlePart`. It is required.
3. Write the **Placement rules**: a list of rules in JSON format. Each rule says which shapes it applies to and where they go.
4. Click **Save**.

The easiest way to start is from the content type editor: the **Edit** page of each part and field of the type has an **Edit placements** button with entries such as *Placement for the ... field in a ... in summary views*. It opens the placement editor with the shape type and a matching rule already filled in; you then only add where the shape goes.

The most common thing to set is `place`: the zone and position (`Content:5`), or a dash (`-`) to hide the shape. For example, with **Shape type** `TitlePart`, this rule hides the title on the pages of Article content items (`Detail` is the display type of an item shown on its own page; `Summary` is used in lists):

```json
[
  {
    "displayType": "Detail",
    "contentType": [ "Article" ],
    "place": "-"
  }
]
```

A rule must contain at least `place`, `shape`, `wrappers` or `alternates`; otherwise saving fails with a message pointing at the rule. If the JSON can't be read, the error says where. The Technical Manual explains every filter and option.

### Edit or delete a placement

Open **Design > Placements**. Each shape type has one entry with **Edit** and **Delete**. In the editor, the shape type can't be changed, and **Delete** at the bottom removes all its rules. Saving an empty list of rules also deletes the placement.

You can't create a second placement for a shape type that already has one: the site says *Placement rules for "..." already exists. Please edit existing rule.*

!!! note
    Your IT team can make placements be stored in a file instead of the database, with the **Placements file storage** (`OrchardCore.Placements.FileStorage`) feature. The screens stay the same.

## Liquid in content

The **Liquid** (`OrchardCore.Liquid`) feature also adds a **Liquid** part and field to content types, where you write Liquid that is shown on the content item's page. Only people with *Manage Liquid templates* can change Liquid content. Liquid is also used in [shortcode templates](shortcodes.md) and in many settings that accept a pattern, such as permalinks.
