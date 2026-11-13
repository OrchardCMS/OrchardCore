---
title: Flows and bags
description: Build the body of a content item from widgets with the Flow part, or keep a collection of items inside it with the Bag part.
technical_manual:
  - reference/modules/Flow/README.md
  - reference/modules/Flow/BagPart.md
  - reference/modules/Widgets/README.md
---

# Flows and bags

The **Flow** and **Bag** parts let a content item hold other content, edited right inside the item's own editor.

- A **Flow** builds the body of a page from **widgets**: paragraphs, images, buttons, quotes, forms, containers. You stack them, drag them into order, and set the width and alignment of each one. The **Page** type of the Blog recipe uses a flow.
- A **Bag** holds a collection of items of the types you choose, for example the slides of a slide show, the members of a team or the questions of an FAQ. The theme decides how the collection looks on the site.

The widgets and items live inside the content item: they have no page of their own, don't appear in **Content > Content Items**, and are saved, published and versioned together with the item.

| | |
| --- | --- |
| **Menu** | The editor of a content item whose type has a **Flow** or **Bag** part (**Content > Content Items**). Settings: **Design > Content Definition > Content Types** |
| **Permission** | The permission to edit the content item; *Edit content types* to change the part settings |
| **Feature** | **Flows** (`OrchardCore.Flows`), which also turns on **Widgets** (`OrchardCore.Widgets`) |


## Add a widget to a flow

1. Open the content item's editor, for example a **Page**.
2. In the **Flow** box, click the **+** button (**Add Widget**) under the widgets.
3. Pick the widget type in the list, for example **Paragraph**.
4. Fill in the widget's editor, which opens in place.
5. Add more widgets the same way, then click **Publish** or **Save Draft** for the whole content item.

To add a widget between two others, click the **+** button (**Insert Widget**) in the header of a widget: the new widget is inserted just before it.

When the flow uses the **Blocks** editor, the **+** button opens a **Select Widget** window instead of a list: search by name, browse the categories on the left, and click **Add** on a widget's card.

!!! note
    The list offers only content types with the **Widget** stereotype, limited to the ones chosen in the part settings. When a widget type is **Securable**, the site saves your additions, changes and removals of those widgets only if your role has the matching permissions for that type.

## Arrange widgets in a flow

Each widget has a header bar with these controls:

| Control | What it does |
| --- | --- |
| Move handle (four arrows) | Drag the widget up or down to reorder the flow. |
| Arrow buttons | Collapse or expand the widget's editor. |
| **Toggle all widgets** | Collapses or expands every widget of the flow at once. |
| Alignment menu | **Left**, **Center**, **Right**, **Justify** or **Inherit**: how the widget is aligned on the page. |
| Size menu | **25%**, **33%**, **50%**, **66%**, **75%** or **100%** of the width. Widgets narrower than 100% sit side by side when there is room. |
| **+** (**Insert Widget**) | Inserts a new widget just before this one. |
| Trash button | Removes the widget, after you confirm. |

A widget starts at **100%** width with the default alignment set in the part settings (**Justify** when none is set). Changes are saved when you save the content item.

## Add items to a bag

1. Open the content item's editor.
2. In the bag's box, click **Add ...** (for example **Add Slide**) when the bag holds one type, or **Add Item** and pick the type when it holds several.
3. Fill in the new item's editor, which opens in place.
4. Click **Publish** or **Save Draft** for the whole content item.

With the **Blocks** editor, the button reads **Add Block** (or the text set in the settings) and opens a **Select Block** window where you pick the type.

Drag an item by its handle to reorder the bag, use the arrow buttons to collapse or expand it, and click the trash button to remove it after you confirm.

## Set up a Flow part

1. Open **Design > Content Definition > Content Types** and click **Edit** on the type.
2. Add the part with **Add Parts** (**Flow**), or click **Edit** on its row if the type already has it.
3. Fill in the settings and click **Save**.

| Setting | What it does |
| --- | --- |
| **Restrict Content Types** | The widget types editors can add. Leave everything unticked to allow every widget type. |
| **Collapse Contained Items** | Widgets open collapsed when the editor loads, which keeps long pages tidy. |
| **What default alignment should be used?** | The alignment of new widgets: **Left**, **Right**, **Center**, **Justify** or **Inherit**. |
| **What type of editor should be used?** | **Standard** (a drop-down list of widgets) or **Blocks** (a picker window with search, categories and pictures). |

The **Blocks** picker shows each widget type's **Description**, **Category** and **Thumbnail**, which you set in the widget type's settings (see [Content types](content-types.md#change-a-content-types-settings)).

## Set up a Bag part

The **Bag** part is reusable: a type can have several bags, each with its own name (add them with **Add Named Part**, see [Content types](content-types.md#add-a-named-part)).

1. Open the type editor and add the part, or click **Edit** on its row.
2. Fill in the settings and click **Save**.

| Setting | What it does |
| --- | --- |
| **Contained Content Types** | Required. Choose **Content Types** to tick the types the bag can hold (at least one), or **Stereotype** to allow all content types of one or more stereotypes. |
| **Stereotypes** | Required when you chose **Stereotype**. One or more stereotypes, separated by commas, for example `Widget`. |
| **Display Type** | How each item is rendered on the site. **Summary** when empty. Change it only if your theme expects another display type. |
| **Collapse Contained Items** | Items open collapsed when the editor loads. |
| **What type of editor should be used?** | **Standard** or **Blocks**. With **Blocks**, you can also set the **Add Button Text** and **Modal Title Text** (the title of the picker window). |

Unlike a flow, a bag can hold any content type, not only widgets. A good habit is to create a dedicated type for the items (for example *Slide*, not **Creatable** or **Listable**) so they don't show elsewhere in the admin.

## When a widget type is deleted

If a widget's content type no longer exists (or is no longer allowed in the flow), the editor leaves the widget out and shows a warning that it has no matching content type definition and that publishing may erase it. Fix the content type first, or ask your site builder, before you save or publish the item.

!!! example "See it in practice"
    [Build a landing page](../use-cases/landing-page-with-widgets.md) walks through this step by step.
