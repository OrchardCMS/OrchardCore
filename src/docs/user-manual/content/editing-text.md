---
title: Edit text
description: Write the body of a content item with the HTML, rich text, code or Markdown editors, insert images and shortcodes, and understand what the site removes from your HTML.
technical_manual:
  - reference/modules/Html/README.md
  - reference/modules/Markdown/README.md
  - reference/modules/Shortcodes/README.md
  - reference/modules/Sanitizer/README.md
  - reference/modules/Media/README.md
---

# Edit text

Most pages and posts have a body: the main text of the item. Depending on how the content type is set up, you write it in HTML (with a rich text editor that works like a word processor, or with a code editor), or in Markdown, a simple way of writing formatted text with symbols such as `**bold**`. This page explains each editor, how to add images and shortcodes, and what the site cleans out of your text.

| | |
| --- | --- |
| **Menu** | In the editor of any content item whose type has an **HtmlBody** or **MarkdownBody** part, or an **Html** or **Markdown** field |
| **Permission** | Permission to edit the content item, see [Content items](content-items.md) |
| **Feature** | **Html** (`OrchardCore.Html`), **Markdown** (`OrchardCore.Markdown`) |

The site builder chooses the editor for each content type. When you see a different editor from the one you expect, it's a choice in the content type, not something you can change while writing.

## Write in an HTML editor

The **HtmlBody** box (or an **Html** field) uses one of these editors:

| Editor | What you see |
| --- | --- |
| **Standard** | A code editor with colored HTML, for people who write HTML tags. |
| **Wysiwyg editor** | A rich text editor with a toolbar: headings, bold, italic, links, lists, alignment and so on. What you see is close to the result. |
| **Trumbowyg editor** | The same rich text editor, with a toolbar the site builder can change. |
| **Monaco editor** | An advanced code editor (the one used by Visual Studio Code), for HTML. |
| **Multiline** | A plain text box. Only for **Html** fields. |

In the rich text editors:

- Select text and click a toolbar button to format it.
- Click the **View HTML** button (`<>`) to see and edit the HTML code behind the text. Click it again to go back.
- Click the full-screen button to give the editor the whole window.

## Write in Markdown

The **MarkdownBody** box (or a **Markdown** field) uses one of these editors:

| Editor | What you see |
| --- | --- |
| **Standard** | A plain text box where you type Markdown. |
| **Wysiwyg editor** | A Markdown editor with a toolbar (**Bold**, **Italic**, **Strikethrough**, **Heading**, **Code**, **Quote**, **Create Link**, **Insert Shortcode**, **Insert Image**, lists, **Insert Table**, **Insert Horizontal Line**), a **Toggle Preview** button to see the result, **Toggle Side by Side** to write and see the result at the same time, and **Toggle Fullscreen**. **Markdown Guide** opens a help page on Markdown. |

A few Markdown basics:

| You type | You get |
| --- | --- |
| `**bold**` | **bold** |
| `*italic*` | *italic* |
| `## Heading` | A heading |
| `- item` | A bulleted list |
| `[link text](https://example.com)` | A link |

## Insert an image or other media

In the rich text editors and the Markdown **Wysiwyg editor**:

1. Place the cursor where the image should go.
2. Click **Insert Media** (in the HTML editors) or **Insert Image** (in the Markdown editor).
3. In the media library window, pick one or more files, or upload new ones. See [Media library](../media/media-library.md).
4. Confirm your choice.

The editor adds an `[image]` shortcode with the file path, for example `[image]photos/beach.jpg[/image]`. The site turns it into the picture when the page is shown. In the **Trumbowyg editor**, the site builder can choose **Insert Media with url** instead: then the editor adds the picture itself, using the file's address.

In the other editors, type the shortcode yourself, or insert it with the shortcode button (see below).

## Insert a shortcode

A shortcode is a short code in square brackets that the site replaces with something richer when the page is shown, such as an image, a link to a media file, or text in one language only.

1. Place the cursor where the shortcode should go.
2. Click **Insert Shortcode** in the toolbar of a rich text or Markdown editor. In the **Standard** HTML editor, the **Standard** Markdown editor and the **Multiline** editor, click the small code button (`</>`) at the corner of the box.
3. In the **Available Shortcodes** window, find the shortcode: type in **Filter**, or click a category on the left, such as **HTML Content**, **Media** or **Localization**. Each card shows how to use the shortcode and its options.
4. Click **Add** on the card.

The shortcode's starting text is placed at the cursor. Fill in the blanks, for example the file name between `[image]` and `[/image]`. The built-in shortcodes are:

| Shortcode | What it does | Example |
| --- | --- | --- |
| `image` | Shows an image from the media library. Options: `width`, `height`, `mode`, `class`, `alt`. | `[image width="300"]photos/beach.jpg[/image]` |
| `asset_url` | Gives the address of a media file, for a link. Options: `width`, `height`, `mode`. | `[asset_url]docs/price-list.pdf[/asset_url]` |
| `locale` | Shows text only in one language. Listed when the **Localization** feature is on. | `[locale en]English text[/locale]` |

The **Monaco editor** has no shortcode button, but shortcodes you type in it work the same. Administrators can add their own shortcodes, which then appear in the window too, see [Shortcodes](../design/shortcodes.md).

## What the site removes from your HTML

To protect visitors, the site cleans the HTML of bodies and fields before it shows them, for example by removing `<script>` tags and other code that could run in the visitor's browser. This is called sanitizing. Your text is saved as you wrote it; the cleaning happens when the page is shown. Shortcodes are processed first, then the result is cleaned.

| Setting | Where | What it does |
| --- | --- | --- |
| **Sanitize Html** | The settings of the **HtmlBody** or **MarkdownBody** part, or of an **Html** or **Markdown** field, in the content type | On by default. Turn it off only for content you fully trust, such as a page where a site builder adds an embed code. |

Which tags and attributes are allowed is set by your IT team, see the Technical Manual.

!!! warning "The Trumbowyg editor cleans HTML when it opens"
    The **Trumbowyg editor** cleans the HTML as it loads it into the editor, even when **Sanitize Html** is off. If the body holds code such as a `<script>` tag, saving from this editor removes it. Use the **Standard** or **Monaco** editor for such content.

## Choose the editor of a content type (site builders)

| | |
| --- | --- |
| **Menu** | **Design > Content Definition > Content Types**, then **Edit** on the type |
| **Permission** | *Edit content types.* |

1. Open **Design > Content Definition > Content Types** and click **Edit** on the content type.
2. Click **Edit** next to the **HtmlBody** or **MarkdownBody** part, or next to an **Html** or **Markdown** field.
3. Choose an editor in **What type of editor should be used?**.
4. Set the options that appear for that editor (see the table).
5. Click **Save**.

| Option | Shown for | What it does |
| --- | --- | --- |
| **Sanitize Html** | Every editor | Cleans the HTML before it is shown, see above. |
| **Hint** | **Html** and **Markdown** fields | A help text shown under the box. |
| **Insert Media with url** | **Trumbowyg editor** | Inserts pictures as images with their address, instead of an `[image]` shortcode. |
| **Enter the editor options** | **Trumbowyg editor**, Markdown **Wysiwyg editor** | The toolbar and behavior of the editor, written in the editor's own configuration format. Follow the **Documentation for options** link, or ask a developer. |
| **Enter the editor options, language is always set to `html` when saved.** | **Monaco editor** | The behavior of the code editor, in its configuration format. |

For the parts and fields of content types in general, see [Content types](content-types.md).
