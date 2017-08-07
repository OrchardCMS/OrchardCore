# Title (Orchard.Title)

The Title module provides a **Title Part** that lets user define a title for a content item. It also defines the `DisplayText` property
of the `ContentItemMetadata` aspect.

## Theming

The following shapes are rendered when the **Title Part** is attached to a content type.

| Name | Display Type | Default Location | Model Type |
| ------| ------------ |----------------- | ---------- |
| `TitlePart` | `Detail` | `Header:5` | `dynamic` |
| `TitlePart_Summary` | `Summary` | `Header:10` | `dynamic` |

### View Model

The following properties are available in the **Title Part** shapes.

| Name | Type | Description |
| -----| ---- |------------ |
| `Title` | `string` | The title property of the part.
