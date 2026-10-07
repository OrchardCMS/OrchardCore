# Media Picker

The Media Picker is the dialog that lets users browse the [Media Library](../../reference/modules/Media/README.md) and select one or more assets, without leaving the page they are editing. It appears everywhere a media asset can be referenced:

- In the editor of a **Media Field** attached to a content type.
- In the toolbar of the **HTML editors** (`HtmlBodyPart`, `HtmlField`) and of the **Markdown editor** (`MarkdownBodyPart`), to insert an image into the body text.

It requires the `OrchardCore.Media` feature to be enabled, and the current user needs a permission to access the Media Library (for example `ManageMedia`, or `ManageOwnMedia` for the user's own folder).

## Using the Media Picker with a Media Field

Add a *Media Field* to a content type (or content part) from `Content → Content Definition` in the admin. When editing a content item of that type, the field displays an empty area with a `+` button that opens the Media Picker. From the dialog you can navigate folders, search, upload new files, and select existing ones.

### Field settings

The behavior of the picker is configured in the field's settings:

| Setting | Description |
| --- | --- |
| `Multiple` | Allow selecting more than one asset. |
| `Allow media text` | Let the user type a text for each selected asset (typically used as the `alt` text of images). |
| `Allow anchors` | Let the user set a focal point on each selected image, exposed to the `resize_url` filter's `anchor` argument. |
| `Media types` | Restrict which kinds of assets can be picked (image, video, audio, document...). |

### Editors

The Media Field ships with several editors, selectable in the field settings:

- **Standard**: picks assets from the shared Media Library.
- **Attached**: uploads files that are owned by the content item rather than picked from the shared library. The files are stored under the `mediafields/` folder and managed with the dedicated `ManageAttachedMediaFieldsFolder` permission.
- **Gallery**: like Standard, with an editor better suited to managing a large set of images.

## Using the Media Picker from a text editor

The Wysiwyg and Trumbowyg editors of the `HtmlBodyPart` and `HtmlField`, and the Markdown editor of the `MarkdownBodyPart`, include a toolbar button that opens the same Media Picker. Selecting an asset inserts an `[image]` [shortcode](../../reference/modules/Shortcodes/README.md) at the cursor position:

```text
[image]sunset.jpg[/image]
```

The shortcode is converted to a full `<img>` tag pointing to the media file when the content is rendered, so the stored body remains portable across environments (no hardcoded URLs).

## Displaying the selected media

The selected assets are stored in the field as relative paths (`Paths`), together with the optional media texts (`MediaTexts`). To render them in a custom template, combine them with the [media Liquid filters](../../reference/modules/Media/README.md#html-filters):

```liquid
{% assign field = Model.ContentItem.Content.Article.Image %}
{% for path in field.Paths %}
    <img src="{{ path | asset_url | resize_url: width: 480 }}" alt="{{ field.MediaTexts[forloop.index0] }}" />
{% endfor %}
```

Or in Razor with the media tag helpers:

```html
<img asset-src="@Model.Field.Paths[0]" img-width="480" alt="@Model.Field.MediaTexts[0]" />
```

See the [Media module documentation](../../reference/modules/Media/README.md) for the full list of filters, tag helpers, and resizing options.
