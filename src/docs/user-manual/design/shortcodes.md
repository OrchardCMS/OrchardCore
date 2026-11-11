---
title: Shortcodes
description: Insert ready-made snippets such as images or language-specific text into your content with shortcodes, and create your own shortcodes with Liquid.
technical_manual:
  - reference/modules/Shortcodes/README.md
  - reference/modules/Liquid/README.md
---

# Shortcodes

A shortcode is a short word in square brackets, such as `[image]photo.jpg[/image]`, that you type into the text of a content item. When the page is shown, the site replaces it with something richer: an image from the media library, a block of HTML, text in the visitor's language. Shortcodes let editors add these things without writing HTML. You can use the built-in shortcodes, and create your own on **Design > Shortcodes**.

| | |
| --- | --- |
| **Menu** | **Design > Shortcodes** |
| **Permission** | *Manage shortcode templates* |
| **Feature** | **Shortcodes** (`OrchardCore.Shortcodes`) to use shortcodes; **Shortcode Templates** (`OrchardCore.Shortcodes.Templates`) to create your own |


!!! warning
    *Manage shortcode templates* is marked security critical: a shortcode template runs Liquid on every page that uses it. Give it only to people you trust.

## Where shortcodes work

Shortcodes are replaced in the HTML and Markdown editors of content items: the HTML body and Markdown body parts, and HTML and Markdown fields, which includes most widgets with text, such as the Paragraph widget. The SEO fields of a content item accept them too. Typed anywhere else, such as in a title or a text field, a shortcode is shown as plain text.

## Insert a shortcode

1. Edit the content item and put the cursor where the shortcode should go.
2. Open the shortcode list:
    - In the rich text (WYSIWYG) and Markdown editors, click the **Insert Shortcode** button of the toolbar.
    - In plain HTML or Markdown text boxes and code editors, click the small code icon (`</>`) at the corner of the box.
3. The **Available Shortcodes** window lists every shortcode with its hint and an example. Click a category on the left (**All**, **HTML Content**, **Media**, **Localization**, ...) or type in **Filter** to narrow the list.
4. Click **Add** on the shortcode you want. Its starting text, such as `[image] [/image]`, is inserted at the cursor.
5. Complete it, for example `[image]blog/photo.jpg[/image]`, and save or publish the content item.

You can also type shortcodes by hand. A shortcode has a name in brackets, optional arguments such as `width=300`, and often some text and a closing tag: `[name argument=value]text[/name]`.

## Built-in shortcodes

| Shortcode | What it does | Example |
| --- | --- | --- |
| `[image]` | Shows an image from the media library. Arguments: `width`, `height` and `mode` to resize it, `class` and `alt` for the HTML. Also `quality`, `format` and `append_version`. | `[image width=300 alt="Our team"]team/photo.jpg[/image]` |
| `[asset_url]` | Gives the address of a media library file, for example inside a link. Arguments: `width`, `height`, `mode`. | `<a href="[asset_url]docs/price-list.pdf[/asset_url]">Price list</a>` |
| `[locale]` | Shows its text only when the page is in the given language. With a second argument `false`, a page in a related language (such as `fr-CA` for `fr`) doesn't match. | `[locale en]Hello[/locale][locale fr]Bonjour[/locale]` |

- The path in `[image]` and `[asset_url]` is the file's path in the media library, such as `blog/photo.jpg` for a file in the `blog` folder. Paths that start with `http`, `//` or `~/` are used as they are. These shortcodes come with the **Media** feature.
- `[locale]` is available when the **Localization** feature is on. See [Localization](../content/localization.md).
- The older `[media]` shortcode still works like `[image]` but is no longer listed.

## Create a shortcode template

A shortcode template is your own shortcode, written in Liquid. For example, a `[primary]` shortcode that shows text in the theme's main color, or a `[site_name]` shortcode that shows the site's name.

1. Open **Design > Shortcodes** and click **Add Shortcode Template**.
2. Fill in the fields below. As you type, a preview card shows how the shortcode will appear in the **Available Shortcodes** window.
3. Click **Save**, or open the arrow next to it and click **and continue** to stay on the page.

| Field | What it does |
| --- | --- |
| **Name** | Required. The name editors type in brackets, without the brackets, such as `site_name`. Must be unique and a valid shortcode name: no spaces or brackets. A template with the name of a built-in shortcode replaces it. |
| **Hint** | A short explanation shown in **Available Shortcodes**. |
| **Usage** | An example of how to use it, shown in **Available Shortcodes**. Simple HTML is allowed, for example a small table of arguments. |
| **Categories** | The categories it is listed under in **Available Shortcodes**. Pick existing ones or type a new one. |
| **Default Shortcode Value** | The text inserted when an editor clicks **Add**. Leave it empty to insert `[name]`. |
| **Content** | Required. The Liquid template that produces the output. It must be valid Liquid, or saving fails with the error. |

In **Content**, these values are available:

| Value | What it holds |
| --- | --- |
| `Args` | The arguments the editor typed, such as `Args.text` for `[primary text="Hi"]`. |
| `Content` | The text between the opening and closing tags, if any. |
| `Context` | Information from the place the shortcode is used, such as `Context.ContentItem` when the editor passes the content item. |

For example, a template named `primary` with this **Content** turns `[primary]Important[/primary]` or `[primary text="Important"]` into text in the theme's primary color:

```liquid
{% capture output %}
  {% if Args.text != nil %}
    <span class="text-primary">{{ Args.text }}</span>
  {% else %}
    <span class="text-primary">{{ Content }}</span>
  {% endif %}
{% endcapture %}
{{ output | sanitize | raw }}
```

The `sanitize` filter removes anything dangerous, such as scripts, from what editors typed.

## Edit or delete shortcode templates

Open **Design > Shortcodes**. Each template shows its name and categories, with **Edit** and **Delete** buttons. Type in **Search** and press Enter to filter. To delete several, tick them and choose **Delete** from the **Actions** menu.

When you delete a template, content that still uses its shortcode shows the shortcode as plain text, such as `[site_name]`.
