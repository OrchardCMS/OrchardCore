# Title (OrchardCore.Title)

The Title module provides a **Title Part** that lets user define a title for a content item.
It also defines the `DisplayText` property of the `ContentItemMetadata` aspect.

## Theming

The following shapes are rendered when the **Title Part** is attached to a content type.

| Name | Display Type | Default Location | Model Type |
| ------| ------------ |----------------- | ---------- |
| `TitlePart` | `Detail` | `Header:5` | `TitlePartViewModel` |
| `TitlePart` | `Summary` | `Header:10` | `TitlePartViewModel` |

### View Model

The following properties are available in the `TitlePartViewModel` class.

| Name | Type | Description |
| -----| ---- |------------ |
| `Title` | `string` | The title property of the part.
| `TitlePart` | `TitlePart` | The `TitlePart` instance.
