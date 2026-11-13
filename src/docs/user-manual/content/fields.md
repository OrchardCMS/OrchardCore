---
title: Fields
description: Fill in every kind of field in the content editor, and choose each field's editor, required rule, hint and other settings when you build a content type.
technical_manual:
  - reference/modules/ContentFields/README.md
  - reference/modules/Spatial/README.md
  - reference/modules/Media/README.md
  - reference/modules/Html/README.md
  - reference/modules/Markdown/README.md
  - reference/modules/Taxonomies/README.md
  - reference/modules/ContentLocalization/README.md
  - reference/modules/SQLIndexing/README.md
---

# Fields

Fields are the boxes of a content item beyond its title and body: a subtitle, a date, a price, a picture, a link to another page. Each content type has its own fields. This page explains how to fill in each kind of field, and, for site builders, which settings each field offers.

| | |
| --- | --- |
| **Menu** | The editor of a content item, see [Content items](content-items.md) |
| **Where you set them up** | **Design > Content Definition > Content Types**, **Edit** on the type, then **Edit** next to the field |
| **Permission** | To fill in: permission to edit the content item. To set up: *Edit content types.* |
| **Feature** | **Content Fields** (`OrchardCore.ContentFields`), plus the features named for some fields below |

## How fields work

Each field has a display name, shown as its label in the editor, and an **editor**: the way you fill it in. The same kind of field can look different from one content type to another. For example, a number can be a plain box, a slider or a drop-down list.

When a field is required, its label is marked as required and you can't save the item until it is filled in. The form then shows a message such as *A value is required for Price.* Many fields also show a hint under the box and a placeholder (grey example text) inside it.

### Set up a field (site builders)

1. Open **Design > Content Definition > Content Types** and click **Edit** on the content type.
2. Click **Edit** next to the field. To add a field, click **Add Field**, see [Content types](content-types.md).
3. Choose the editor in **What type of editor should be used?** and, when offered, how it is shown on the site in **What type of display mode should be used?**.
4. Fill in the settings of the field (described for each field below). The options change with the editor you choose.
5. Click **Save**.

Settings that most fields share:

| Setting | What it does |
| --- | --- |
| **Required** | The item can't be saved while the field is empty. |
| **Hint** | Help text shown under the field. |
| **Placeholder** | Grey example text shown inside an empty box. |

## Text field

A single line of text, such as a subtitle or a reference number.

| Editor | How you fill it in |
| --- | --- |
| **Standard** | A one-line box. |
| **Multi-line** | A larger box for several lines. |
| **Email** | A box that expects an email address. |
| **Phone** | A box for a phone number. |
| **Url** | A box that expects a web address. |
| **Color** | A color picker, with an **Opacity** slider. |
| **Icon picker** | A button that opens a list of icons to choose from. |
| **Predefined List** | A choice from a fixed list, as a **Drop down list** or as **Radio buttons**. |
| **Code Mirror** | A code editor. |
| **Monaco editor** | An advanced code editor. |

Settings: **Required**, **Hint**, **Placeholder**, and:

| Setting | What it does |
| --- | --- |
| **Default value** | Text filled in when someone creates an item. |
| **Minimum length**, **Maximum length** | The fewest and most characters allowed. Leave blank for no limit. |
| **Type** | **Editable** (the person types the value), **Generated Hidden** or **Generated Disabled**: the site fills the value in from the **Pattern** each time the item is saved, and the box is hidden or greyed out. |
| **Pattern** | Required for the generated types. A Liquid template that builds the value, for example `{{ ContentItem.DisplayText }}`. Ask a developer for help with Liquid. |
| **What type of list editor should be used?**, **Options** | For **Predefined List**: the list style and the choices. Each option has an **Option Label** (what people see) and a **Value** (what is saved). Tick **Default?** on the option chosen for new items. |
| **Enter the editor options.** | For **Monaco editor**: its configuration, for developers. |

On the site, a text field can be shown as plain text (**Standard**) or as a heading (**Header**, with a heading level from **H1** to **H6**).

## Html field and Markdown field

A block of formatted text, like a second body. The editors and settings are the same as for the body, see [Editing text](editing-text.md). The **Html** field also offers a **Multiline** editor, a plain text box. The **Markdown** field comes with the **Markdown** feature (`OrchardCore.Markdown`).

## Boolean field

A yes or no choice, such as *Featured*.

| Editor | How you fill it in |
| --- | --- |
| **Standard** | A checkbox. |
| **Switch** | An on/off switch. |

Settings: **Hint**, **Label** (the text next to the checkbox; the field's name is used when empty), and **Default value is true** (new items start ticked).

## Numeric field

A number, such as a price or a quantity.

| Editor | How you fill it in |
| --- | --- |
| **Number** | A number box. |
| **Spinner** | A number box with buttons to go up and down. |
| **Slider** | A slider. |
| **Range** | A number box with a slider under it. |
| **Select** | A drop-down list of every allowed value, from the minimum to the maximum. |

Settings: **Required**, **Hint**, and:

| Setting | What it does |
| --- | --- |
| **Watermark (placeholder)** | Grey example text in the empty box. |
| **Scale** | How many digits are allowed after the decimal point. 0 means whole numbers only. |
| **Minimum**, **Maximum** | The lowest and highest values allowed. Optional. |
| **Default value** | The value filled in for new items. Optional. |

A value outside the limits, or with too many decimals, is refused with a message such as *The value must be less than 100.*

## Date, Date Time and Time fields

| Field | How you fill it in |
| --- | --- |
| **Date** | A date picker. With the **Localized** editor, the date is typed in your language's date format. |
| **Date Time** | A date and a time. You enter it in your own time zone (yours if the **User Time Zone** feature lets you choose one, otherwise the site's), and the site stores it so it shows correctly for everyone. |
| **Time** | A time of day. |

Settings: **Required**, **Hint**. The **Time** field also has **Step**, which lets editors enter seconds or fractions of a second: for seconds use a number that divides 60, such as `15`.

## Link field

A link: an address, a text to click and where it opens.

You fill in **Url** (a full address such as `https://example.com`, or a page of the site such as `/about`), **Link text** and **Target**. **Target** decides where the link opens; you can pick a value from its list, for example *Opens the linked document in a new window or tab.* An address that isn't valid is refused with *... is an invalid url.*

Settings:

| Setting | What it does |
| --- | --- |
| **Required** | The url must be filled in. |
| **Text** | **Text is optional**, **Text is required**, **Use the default value** (everyone gets **Default value of the text**, and the box is hidden) or **Use the url of the link** (the address is shown as the text, and the box is hidden). |
| **Hint for the url**, **Hint for the text** | Help texts under each box. |
| **Watermark for the url**, **Watermark for the text** | Grey example texts in each empty box. |
| **Default value of the url**, **Default value of the text**, **Default value of the Target** | Values filled in for new items. |

## Multi Text field

Several choices from a fixed list, such as the days a shop is open.

| Editor | How you fill it in |
| --- | --- |
| **Standard** | A list where you select several options (hold **Ctrl**, or **Cmd** on a Mac, to select more than one). |
| **Checkbox List** | A checkbox for each option. |
| **Picker** | A search box: type to find an option and click it. Chosen options appear as tags in the box; click a tag's **x** to remove it. |

Settings: **Required**, **Hint**, and **Options**: the list of choices, each with an **Option Label**, a **Value** and **Default?**. Click **Add an option** to add one, drag options to reorder them, and use **Edit Data** to edit the whole list as text.

## Content Picker field

Links to other content items, such as related articles or the author's page.

1. Click in the box and type part of a title. The list shows up to 50 matching items.
2. Click an item to add it. Items not published yet are marked *Not published*.
3. Picked items are listed under the box. Click an item's title to open it, or its delete (trash) button to remove it. When several items are allowed, drag them by the handle to change their order.

| Editor | How it searches |
| --- | --- |
| **Standard** | Searches the titles and content type names of the latest versions. |
| **Lucene** | Searches a Lucene index. Needs the **Lucene Content Picker** feature (`OrchardCore.Search.Lucene.ContentPicker`). |
| **Elasticsearch** | Searches an Elasticsearch index. Needs the **Elasticsearch Content Picker** feature (`OrchardCore.Search.Elasticsearch.ContentPicker`). |

Settings:

| Setting | What it does |
| --- | --- |
| **Required** | At least one item must be picked. |
| **Allow multiple** | More than one item can be picked. |
| **Contained Content Types** | Which items can be picked: **Display All Content Types**, chosen **Content Types**, or **Stereotype** (all content types of the stereotypes you type in **Stereotypes**, separated by commas). |
| **Placeholder**, **Hint** | See above. |
| **Title Pattern** | How each item is named in the picker, as a Liquid template. Leave the default unless a developer suggests otherwise. |
| **Index** | For **Lucene** and **Elasticsearch**: the search index to use. See [Site search](../search/site-search.md). |

## User Picker field

Links to user accounts, such as the people responsible for a project. Needs the **Users** feature.

Type part of a user name and click a user to add them. Picked users are listed under the box, with a delete (trash) button to remove one.

Settings: **Required**, **Allow multiple**, **Placeholder**, **Hint**, and either **Display All Users** or a list of **Roles**: only users with one of the ticked roles can be picked. On the site the field can show the users (**Standard**) or only their names (**User Names**).

## Media field

Pictures and other files from the media library, such as a banner image.

1. Click **Add media** (the plus button) and pick files in the media library window, or upload new ones. See [Media library](../media/media-library.md).
2. To remove a file, select it and click **Remove**.
3. When allowed, click **Media text** to type a text for the file (for example a caption or alternative text), and **Anchor** to choose the point of the picture that stays visible when it is cropped.

| Editor | How it works |
| --- | --- |
| **Standard** | Picks files from the media library. |
| **Attached** | Uploads files straight from your computer (choose them, or drag them onto the field). They belong to this item only: they are stored in a folder of their own for the item and removed when the item is deleted. |
| **Gallery** | Shows the picked files as cards or as a list, with the actions next to each file. |

Settings:

| Setting | What it does |
| --- | --- |
| **Required** | At least one file must be picked. |
| **Multiple** | More than one file can be picked. |
| **Allow media text** | Shows the **Media text** button for each file. |
| **Allow anchors** | Shows the **Anchor** button for each file. |
| **Hint** | Help text. |
| **Allow all default media types** | Any file the media library accepts can be picked. Untick it to tick the file types allowed, for example only images. A file of another type is refused with *Media extension is not allowed.* |

The **Media field** comes with the **Media** feature (`OrchardCore.Media`).

## Taxonomy field

Categories or tags from a taxonomy, such as the topics of a blog post. With the **Standard** editor you tick terms in a list; with the **Tags** editor you type tags and, when allowed, create new ones as you type. Settings include **Taxonomy** (which taxonomy to use), **Unique** (only one term), **Leaves only**, **Required**, **Hint** and **Placeholder**. See [Taxonomies](taxonomies.md).

## Localization Set Content Picker field

Like the content picker, but it links to a content item in all its languages at once, so each translation of your item points to the matching translation of the picked item. Needs the **Content Localization** feature (`OrchardCore.ContentLocalization`). Settings: **Required**, **Allow multiple**, **Content Types**, **Hint**. See [Localization](localization.md).

## YouTube field

A YouTube video, shown on the page in a video box.

Paste the full address of the video, such as `https://www.youtube.com/watch?v=abc123` or `https://youtu.be/abc123`. An address of another form is refused with *The format of the url is invalid*.

Settings: **Required**, **Placeholder**, **Hint**, **Width** and **Height** of the video box.

## Geo Point field

A place on a map, given by its latitude and longitude, such as the address of a shop. Needs the **Spatial** feature (`OrchardCore.Spatial`), which also turns on Lucene search.

| Editor | How you fill it in |
| --- | --- |
| **Standard** | Type the **Latitude** (between -90 and 90) and **Longitude** (between -180 and 180). |
| **Leaflet editor** | The same boxes with a map under them: click the map or drag the marker to set the place. |

Settings: **Required**, **Hint**.

## Content Fields Indexing (SQL)

The **Content Fields Indexing (SQL)** feature (`OrchardCore.ContentFields.Indexing.SQL`) copies the values of text, Boolean, number, date, date and time, time, link, HTML, multi text and content picker fields into database tables. **Content Fields Indexing (SQL) - User Picker** (`OrchardCore.ContentFields.Indexing.SQL.UserPicker`) does the same for user picker fields.

Nothing changes in the editor. These tables let developers and site builders write SQL queries that list or filter content by field values, for example all events after a date, in **Search > Queries**. See [Queries](../search/queries.md). Turn them on when a query needs them; your developer will tell you.
