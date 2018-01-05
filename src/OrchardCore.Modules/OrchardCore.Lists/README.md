# Lists (OrchardCore.Lists)

## Theming

### Shapes

These shapes are available for theming when a `ListPart` is attached to a content item.

| Name | Display Type | Default Location | Model Type |
| ------| ------------ |----------------- | ---------- |
| `ListPart` | `Detail`, `DetailAdmin` | `Content:10` | `ListPartViewModel` |

### ListPartViewModel

The following properties are available on the `ListPartViewModel` class.

| Property | Type | Description |
| --------- | ---- |------------ |
| `ListPart` | `ListPart` | The `ListPart` instance |
| `ContentItems` | `IEnumerable<ContentItem>` | The content items the part is made of |
| `ContainedContentTypeDefinitions` | IEnumerable<ContentTypeDefinition> | The content types the list accepts |
| `Context` | `BuildPartDisplayContext` | The current display context |
| `Pager` | `dynamic` | The pager for the list |

### ListPart

The following properties are available on the `ListPart` class.

| Name | Type | Description |
| -----| ---- |------------ |
| `Content` | The raw content of the part |
| `ContentItem` | The content item containing this part |

## CREDITS

### Trumbowyg
<https://github.com/Alex-D/Trumbowyg>  
Copyright (c) 2012-2016 Alexandre Demode (Alex-D)  
License: MIT
