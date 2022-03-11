# Shortcodes (`OrchardCore.Shortcodes`)

Adds Shortcode capabilities. 

Shortcodes are small pieces of code wrapped into \[brackets\] that can add some behavior to content editors, like embedding media files.

Shortcodes can be implemented by enabling the Shortcode Templates feature or through code.

## Shortcode Templates

Shortcode templates with [Liquid](../Liquid/) are created through the _Design -> Shortcodes_ menu.

Shortcode templates are designed to be able to override a code based Shortcode of the same name.

| Parameter | Description |
| --------- | ----------- |
| `Name` | The name of your Shortcode, without brackets. |
| `Hint` | The hint to display for your Shortcode.
| `Usage` | An html string to describe the usage and arguments for your Shortcode. |
| `Categories` | The categories your Shortcode falls under. |
| `Return Shortcode` | The Shortcode value to return from the Shortcode picker when selected. Defaults to Name. |
| `Content` | The Liquid template for your Shortcode. |

### Template Arguments

| Parameter | Description |
| --------- | ----------- |
| `Args` | The arguments provided the user, if any. |
| `Content` | The inner content provided by the user, if any.
| `Context` | The context made available to the Shortcode from the caller, e.g. an `HtmlBodyPart`. |

### Example Shortcode Templates :

#### `[display_text]`

| Parameter | Value |
| --------- | ----------- |
| `Name` | display_text |
| `Hint` | Returns the display text of the content item. |
| `Usage` | [display_text] |
| `Content` | `{{ Context.ContentItem.DisplayText }}`<br>`{{ More }}` |

!!! note
    The `ContentItem` `Context` is only available when the caller, i.e. an `HtmlBodyPart`, has passed the `ContentItem` value to the `Context`. 

#### `[site_name]`

| Parameter | Value |
| --------- | ----------- |
| `Name` | site_name |
| `Hint` | Returns the site name. |
| `Usage` | [site_name] |
| `Content` | `{{ Site.SiteName }}` |

####  `[primary]`

| Parameter | Value |
| --------- | ----------- |
| `Name` | primary |
| `Hint` | Formats text in the themes primary color. |
| `Usage` | [primary text]&lt;br&lt;[primary]text[/primary] |
| `Content` | `{% capture output %}`<br>`{% if Args.Text != nil %}`<br>`<span class="text-primary">{{Args.Text}}</span>`<br>`{% else %}`<br>`<span class="text-primary">{{Content}}</span>`<br>`{% endif %}`<br>`{% endcapture %}`<br>`{{ output | sanitize | raw }}` |

## Shortcodes via code

### Shortcode Delegate

Shortcodes can be registered in code via a `ShortcodeDelegate` using the `AddShortcode` extension method.

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `Name` | `string` | The name of the Shortcode. |
| `Shortcode` | `ShortcodeDelegate` | The Shortcode Delegate. |
| `Describe` | `Action<ShortcodeOption>` | Optionally a Description of the Shortcode. |

In this example, we register a `[bold]` Shortcode Delegate and describe the Shortcode.

``` csharp
services.AddShortcode("bold", (args, content, ctx) => {
    var text = args.NamedOrDefault("text");
    if (!String.IsNullOrEmpty(text))
    {
        content = text;
    }

    return new ValueTask<string>($"<b>{content}</b>");
}, describe => {
    describe.DefaultValue = "[bold text-here]";
    describe.Hint = "Add bold formatting with a shortcode.";
    describe.Usage = "[bold 'your bold content here']";
    describe.Categories = new string[] { "HTML Content" };
    };
});
```

### `IShortcodeProvider`

Shortcodes may also be used by implementing the `IShortcodeProvider` interface and registered with the `AddShortcode` extension method.

In this example we register an `ImageShortcodeProvider` as `[image]` and describe the Shortcode.

``` csharp
services.AddShortcode<ImageShortcodeProvider>("image", describe => {
    describe.DefaultValue = "[image] [/image]";
    describe.Hint = "Add a image from the media library.";
    describe.Usage = 
@"[image]foo.jpg[/image]<br>
<table>
  <tr>
    <td>Args:</td>
    <td>width, height, mode</td>
  </tr>
  <tr>
    <td></td>
    <td>class, alt</td>
  </tr>
</table>"; 
    describe.Categories = new string[] { "HTML Content", "Media" };
    };
});
```

!!! note
    When upgrading from version `1.0.0-rc2-13450` you may need to re-enable the Shortcodes feature, through _Configuration -> Features_

    The Shortcode Templates feature is only available from the [Preview Feed](../../../getting-started/preview-package-source)

## Available Shortcodes

### `[image]`

The [image] shortcode renders an image from the site's media library.

Example
```
[image alt="My lovely image"]my-image.jpg[/image]
```
This will render an image tag for the file `my-image.jpg` in the site's media folder.

The following parameters can be used:

- **alt:** Adds alternative text to your image for the benefit of readers who can't see the image and also good for SEO.
- **class:** Adds an html class attribute to the image tag for styling.
- **format:** Change the file format from the original file. Can be jpeg, png, gif or bmp.
- **quality:** Sets the encoding quality to use for jpeg images. The higher the quality, the larger the file size will be. The value can be from 0 to 100 and defaults to 75.
- **width, height:** The width and height can be set to resize the image. The possible values are limited to prevent malicious clients from creating too many variations of the same image. The values can be 16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048.
- **mode:** The resize mode controls how the image is resized.  
   The options are:
  - **pad:** Pads the resized image to fit the bounds of its container. If only one dimension is passed, the original aspect ratio will be maintained.
  - **boxpad:** Pads the image to fit the bounds of the container without resizing the original source. When downscaling, performs the same functionality as pad.
  - **max** (Default): Constrains the resized image to fit the bounds of its container maintaining the original aspect ratio.
  - **min:** Resizes the image until the shortest side reaches the given dimension. Upscaling is disabled in this mode and the original image will be returned if attempted.
  - **stretch:** Stretches the resized image to fit the bounds of its container.
  - **crop:** Resizes the image using the same functionality as max then removes any image area falling outside the bounds of its container.

### `[asset_url]`

The [asset_url] shortcode returns a relative url from the site's media library.

Example
```
[asset_url]my-image.jpg[/asset_url]
```
This will return a relative url of `/my-tenant/media/my-image.jpg` for the file `my-image.jpg` in the site's media folder.

The following parameters can be used:

- **format:** Change the file format from the original file. Can be jpeg, png, gif or bmp.
- **quality:** Sets the encoding quality to use for jpeg images. The higher the quality, the larger the file size will be. The value can be from 0 to 100 and defaults to 75.
- **width, height:** The width and height can be set to resize the image. The possible values are limited to prevent malicious clients from creating too many variations of the same image. The values can be 16, 32, 50, 100, 160, 240, 480, 600, 1024, 2048.
- **mode:** The resize mode controls how the image is resized.  
   The options are:
  - **pad:** Pads the resized image to fit the bounds of its container. If only one dimension is passed, the original aspect ratio will be maintained.
  - **boxpad:** Pads the image to fit the bounds of the container without resizing the original source. When downscaling, performs the same functionality as pad.
  - **max** (Default): Constrains the resized image to fit the bounds of its container maintaining the original aspect ratio.
  - **min:** Resizes the image until the shortest side reaches the given dimension. Upscaling is disabled in this mode and the original image will be returned if attempted.
  - **stretch:** Stretches the resized image to fit the bounds of its container.
  - **crop:** Resizes the image using the same functionality as max then removes any image area falling outside the bounds of its container.

### `[locale]`

The `locale` shortcode conditionally renders content in the specified language. Output is based on the current thread culture.
This shortcode is only available when the `OrchardCore.Localization` module is enabled. 

Example
```
[locale en]English Text[/locale][locale fr]French Text[/locale]
```

By default, the shortcode will render the content if the current locale is a parent of the specified language. 
For example, if the current locale is `en-CA` and you specified this shortcode: `[locale en]English Text[/locale]` The output will be `English Text`.
You can disable this behavior by passing `false` as the second argument of the shortcode. 
`[locale en false]English Text[/locale]` would render nothing if the current culture is not exactly `en`.

## Rendering Shortcodes

Shortcodes are automatically rendered when using a `Shape` produced by a display driver that supports Shortcodes.

- `HtmlBodyPart`
- `HtmlField`
- `MarkdownBodyPart`
- `MarkdownField`

=== "Liquid"

    ``` liquid
    {{ Model.Content.HtmlBodyPart | shape_render }}
    ```

=== "Razor"

    ``` html
    @await DisplayAsync(Model.Content.HtmlBodyPart)
    ```

Shortcodes can also be rendered via a liquid filter or html helper

=== "Liquid"

    ``` liquid
    {{ Model.ContentItem.Content.RawHtml.Content.Html | shortcode | raw }}
    ```

=== "Razor"

    ``` html
    @Html.Raw(@await Orchard.ShortcodesToHtmlAsync((string)Model.ContentItem.Content.RawHtml.Content.Html))
    ```

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/hsTJSIxUmZo" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/ofPKGsW5Ftg" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
