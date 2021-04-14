# Title (`OrchardCore.Title`)

The `Title` module provides a **Title Part** that lets you customize the DisplayText property of a content item. The DisplayText property is used throughout the Admin interface to help you recognize your content items.

## TitlePart

Attach this part to your content items to customize the DisplayText property of a ContentItem.

### TitlePart Settings

By default, attaching the TitlePart will allow content editors to manually edit the DisplayText (title) of a ContentItem.

You can also generate the Title by specifying a pattern using a Liquid expression.

The Pattern has access to the current ContentItem and is executed on ContentItem update. For example, fields can be used to generate the pattern. The following example uses a __Text field__ named `Name`, on a `Product` content type.

```liquid
{{ ContentItem.Content.Product.Name.Text }}
```

## Theming

The following shapes are rendered when the **Title Part** is attached to a content type.

| Name        | Display Type | Default Location | Model Type           |
| ----------- | ------------ | ---------------- | -------------------- |
| `TitlePart` | `Detail`     | `Header:5`       | `TitlePartViewModel` |
| `TitlePart` | `Summary`    | `Header:10`      | `TitlePartViewModel` |

### View Model

The following properties are available in the `TitlePartViewModel` class.

| Name        | Type        | Description                     |
| ----------- | ----------- | ------------------------------- |
| `Title`     | `string`    | The title property of the part. |
| `TitlePart` | `TitlePart` | The `TitlePart` instance.       |
