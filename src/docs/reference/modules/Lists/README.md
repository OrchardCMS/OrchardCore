# Lists (`OrchardCore.Lists`)

A ListPart allows you to associate content items to a parent container (Ex: A blog contains a list of blog posts).

## Theming

### Shapes

These shapes are available for theming when a `ListPart` is attached to a content item.

| Name | Display Type | Default Location | Model Type |
| ------| ------------ |----------------- | ---------- |
| `ListPart` | `Detail`, `DetailAdmin` | `Content:10` | `ListPartViewModel` |

### `ListPartViewModel`

The following properties are available on the `ListPartViewModel` class.

| Property | Type | Description |
| --------- | ---- |------------ |
| `ListPart` | `ListPart` | The `ListPart` instance. |
| `ContentItems` | `IEnumerable<ContentItem>` | The content items the part is made of. |
| `ContainedContentTypeDefinitions` | `IEnumerable<ContentTypeDefinition>` | The content types the list accepts. |
| `Context` | `BuildPartDisplayContext` | The current display context. |
| `Pager` | `dynamic` | The pager for the list. |

### `ListPart`

The following properties are available on the `ListPart` class.

| Name | Type | Description |
| -----| ---- |------------ |
| `Content` | The raw content of the part. |
| `ContentItem` | The content item containing this part. |

### `ListPartSettings`

The following properties are available on the `ListPartSettings` class.

| Name | Type | Description |
| -----| ---- |------------ |
| `PageSize` | The number of content items returned per page. |
| `EnableOrdering` | Flag to enable drag and drop ordering of content items. |
| `ContainedContentTypes` | The content types that may be contained by this part. |

### Template

The following example is used to render the items of a `ListPart` and customize the pager.  
For instance, it can be set in a file named `Blog-ListPart.liquid` to override the `Blog` content type only.

```liquid
{% for item in Model.ContentItems %}
    {{ item | shape_build_display: "Summary" | shape_render }}
{% endfor %}

{% assign previousText = "Newer Posts" | t %}
{% assign nextText = "Older Posts" | t %}
{% assign previousClass = "previous" | t %}
{% assign nextClass = "next" | t %}

{% shape_pager Model.Pager previous_text: previousText, next_text: nextText,
    previous_class: previousClass, next_class: nextClass %}

{{ Model.Pager | shape_render }}
```

## Orchard Helpers

### QueryListItemsCountAsync

Returns the count of `IEnumerable<ContentItem>` satisfying the given predicate.

### QueryListItemsAsync

Returns the `IEnumerable<ContentItem>` satisfying the given predicate.

## Liquid Tags

### list_count

The `list_count` filter counts published content items for a given `ContentItem` object or explicit `ContentItem` id given as a string in a list.

### list_items

The `list_items` filter loads published content items for a given `ContentItem` object or explicit `ContentItem` id given as a string in a list.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/a3yyR27vdQQ" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Lucene Indexing

**Breaking Change**: Every content item that is contained in a list has a `ContainedPart` associated with it.
The indexed data in Lucene before Orchard Core 1.1 was:

`"Content.ContentItem.ListContentItemId"`

After 1.1 this has changed for also including the display order, so it is now:

`"Content.ContentItem.ContainedPart.ListContentItemId"`   
and  
`"Content.ContentItem.ContainedPart.Order"`