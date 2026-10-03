# Title (`OrchardCore.Title`)

The `Title` module provides a **Title Part** that lets you customize the DisplayText property of a content item. The DisplayText property is used throughout the Admin interface to help you recognize your content items.

## TitlePart

Attach this part to your content items to customize the DisplayText property of a ContentItem.

### TitlePart Settings

By default, attaching the TitlePart will allow content editors to manually edit the DisplayText (title) of a ContentItem.

You can also generate the Title by specifying a pattern using a Liquid expression.

The Pattern has access to the current ContentItem and is executed on ContentItem update. For example, fields can be used to generate the pattern. The following example uses a **Text field** named `Name`, on a `Product` content type.

```liquid
{{ ContentItem.Content.Product.Name.Text }}
```

## Theming

The following shapes are rendered when the **Title Part** is attached to a content type.

| Name        | Display Type | Default Location | Model Type           |
|-------------|--------------|------------------|----------------------|
| `TitlePart` | `Detail`     | `Header:5`       | `TitlePartViewModel` |
| `TitlePart` | `Summary`    | `Header:10`      | `TitlePartViewModel` |

### View Model

The following properties are available in the `TitlePartViewModel` class.

| Name        | Type        | Description                     |
|-------------|-------------|---------------------------------|
| `Title`     | `string`    | The title property of the part. |
| `TitlePart` | `TitlePart` | The `TitlePart` instance.       |

## Placement

For front-end placement, the `TitlePart` shape uses the part name as its differentiator.

For example, to hide the title on the front end:

```json
{
    "TitlePart": [
        {
            "differentiator": "TitlePart",
            "place": "-"
        }
    ]
}
```

To hide or move the **editor** in the admin UI, target the `ContentPart_Edit` wrapper shape instead of `TitlePart_Edit`. The wrapper differentiator is `{ContentType}-{PartName}`.

For example, to hide the `TitlePart` editor row on the `Article` content type editor:

```json
{
    "ContentPart_Edit": [
        {
            "differentiator": "Article-TitlePart",
            "place": "-"
        }
    ]
}
```

`TitlePart_Edit` only targets the inner editor shape. Use `ContentPart_Edit` when you want to hide or move the whole editor row, including its wrapper.
