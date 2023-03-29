# OrchardCore.DisplayManagement

This article is about Display management and placement files.

## Placement files

Any module or theme can contain an optional `placement.json` file providing custom placement logic.

!!! note
    The `placement.json` file must be added at the root of a module or a theme.

### Format

A `placement.json` file contains an object whose properties are shape types. Each of these properties is an array of placement rules.

In the following example, we describe the placement for the `TextField` and `Parts_Contents_Publish` shapes.

```json
{
  "TextField": [ ... ],
  "Parts_Contents_Publish" : [ ... ]
}
```

A placement rule contains two sets of data:

- **Filters** - defines what specific shapes are targeted.
- **Placement information** - the placement information to apply when the filter is matched.

Currently you can filter shapes by:

- Their original type, which is the property name of the placement rule, like `TextField` or `ContentPart`.
- `displayType` (Optional): The display type, like `Summary` and `Detail` for the most common ones.
- `differentiator` (Optional): The differentiator which is used to distinguish shape types that are reused for multiple elements, like field names.

!!! note
    Shape type (placement.json property name) DOES NOT necessarily align with with your part type. For instance, if you created a Content Part `GalleryPart` without a part driver, your shape type will be `ContentPart` with differentiator `GalleryPart`. So your placement.json would look like
```json
{
  "ContentPart": [{
  "place":"SomeZone"
  "differentiator":"GalleryPart"
  }],
  "GalleryPart": [{...}], //this wont work unless you registered a driver for the part
}
```

Additional custom filter providers can be added by implementing `IPlacementNodeFilterProvider`.

For shapes that are built from a content item, you can filter by the following built in filter providers:

- `contentType` (Optional): A single ContentType or Stereotype, or an array of ContentTypes and / or Stereotypes that the content item from which the shape was built should match. `*` maybe used to match all content types starting with the preceding value, i.e. `Art*`.
- `contentPart` (Optional): A single ContentPart or an of array of ContentParts that the content item from which the shape was built should contain.
- `path` (Optional): A single path or an of array of paths that should match the request path.

Placement information consists of:

- `place` (Optional): The actual location of the shape in the rendered zone. A value of `-` will hide the shape, and a value starting with `/` will move the shape to a layout zone.
- `alternates` (Optional): An array of alternate shape types to add to the current shape's metadata.
- `wrappers` (Optional): An array of shape types to use as wrappers for the current shape.
- `shape` (Optional): A substitution shape type.

```json
{
  "TextField": [
    {
      "displayType": "Detail",
      "differentiator": "Article-MyTextField",
      "contentType": [ "Page", "BlogPost" ],
      "contentPart": [ "HtmlBodyPart" ],
      "path": [ "/mypage" ],
      "place": "Content",
      "alternates": [ "TextField_Title" ],
      "wrappers": [ "TextField_Title" ],
      "shape": "AnotherShape"
    }
  ],
}
```

### Placement precedence

The placement info chosen for a shape is based on the following order:

1. The main startup project (This can act as a super theme)
2. Active theme (This will be the active front end theme if you're viewing the front end, or the active admin theme if you're viewing the admin)
3. Modules (Ordered by dependencies)

### Placing Fields

Fields have a custom differentiator as their shape is used in many places.  
It is built using the `Part` it's contained in, and the name of the `Field`.  
For instance, if a field named `MyField` would be added to an `Article` content type, its differentiator would be `Article-MyField`.  
If a field named `City` was added to an `Address` part then its differentiator would be `Address-City`.

## Shape differentiators

You can find information about shape differentiators in the [Templates documentation](../../modules/Templates/README.md#content-field-differentiator)

## Shapes

### What is a shape?

Everything you need to know about Shapes is in [this video](https://youtu.be/gKLjtCIs4GU).

### Rendering a shape

You can use the `<shape>` tag helper to render any shape, even pass properties.

=== "Razor"

    ``` html
    @{
        var intValue = 1;
        var stringValue = "a";
    }

    @await DisplayAsync(await New.MyShape(Foo: 1, Bar: "a"))

    <shape type="MyShape" foo="1" bar="a" />

    <shape type="MyShape" prop-foo="1" bar="a" />

    <shape type="MyShape" prop-foo="@intValue" prop-bar="@stringValue" />
    ```

=== "Liquid"

    ``` liquid
    {% assign customShape = "MyShape" | shape_new %}
    {% shape_add_properties customShape my_string: "String Test 3", my_int: 1 %}
    {{ customShape | shape_render }}

    {% "MyShape" | shape_new | shape_properties: my_int: 3, my_string: "String Test 3" | shape_render %}
    ```

For rendering content items, you could also use the following tag helper.
Note: you need to add `@addTagHelper *, OrchardCore.Contents.TagHelpers` to your `_ViewImports.cshtml` file to load this tag helper. Ensure your project file also has a reference to OrchardCore.Contents.TagHelpers.

=== "Razor"

    ``` html
    <contentitem alias="alias:main-menu" display-type="Detail" />
    ```

=== "Liquid"

    ``` liquid
    {% contentitem alias: "alias:main-menu", display_type: "Detail" %}
    ```

#### Manipulating shape metadata

It's possible to manipulate a shape's metadata by using the `metadata` tag helper as a child of the shape's tag helper. The metadata tag helper allows you to:

- Change the display type
- Add, remove, or clear alternates
- Add, remove, or clear wrappers

Metadata tag helper example:

```xml
<menu alias="alias:main-menu">
    <metadata display-type="summary">
        <clear-alternates />
        <add-alternate name="Menu_Alternate1" />
        <add-alternate name="Menu_Alternate2" />
        <remove-alternate name="Menu_Alternate1" />
        <clear-wrappers />
        <add-wrapper name="Menu_Wrapper1" />
        <add-wrapper name="Menu_Wrapper2" />
        <remove-wrapper name="Menu_Wrapper2" />
    </metadata>
</menu>
```

### Date Time shapes

#### `DateTime`

Renders a `Date` and `Time` value using the timezone of the request.

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `Utc` | `DateTime?` | The date and time to render. If not specified, the current time will be used. |
| `Format` | `string` | The .NET format string. If not specified the long format `dddd, MMMM d, yyyy h:mm:ss tt` will be used. The accepted format can be found at <https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx> |

Tag helper example:

```html
<datetime utc="@contentItem.CreatedUtc" />
```

#### `TimeSpan`

Renders a relative textual representation of a `Date` and `Time` interval.

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `Utc` | `DateTime?` | The initial date and time. If not specified, the current time will be used. |
| `Origin` | `DateTime?` | The current date and time. If not specified, the current time will be used. |

Tag helper example:

```html
<timespan utc="@contentItem.CreatedUtc" />
```

Result:

```text
3 days ago
```

## Editor shape placement

Editor shapes support grouping placement, which allows you to group editor shapes, to create a variety of content editor layouts.

### Supported groupings

- Tabs
- Cards
- Columns

Each grouping works by itself, or can be progressive, so Tabs can support Cards, and / or Columns, and Cards can support Columns.

Groupings are created by applying a modifier and a group name.

### Modifiers

- The Tabs modifier is `#`
- The Cards modifier is `%`
- The Columns modifier is `|`

Each of these modifiers support a position modifier for the group, in the format `;` and Columns support an additional modifier for the column width, of `_`.

To apply a position modifier, or column width modifier, apply the appropriate value to every group name.

Fields or Parts which do not have a grouping will fall into the default `Content` group when other fields apply a grouping.

### Examples

In the following example we place the `MediaField_Edit` shape in a tab called `Media`, and position the `Media` tab first, and the `Content` tab second.

``` json

{
    "MediaField_Edit" : [
        {
            "place" : "Parts:0#Media;0",
            "contentType": [
                "Article"
            ]
        }
    ],
    "HtmlField_Edit" : [
        {
            "place": "Parts:0#Content;1",
            "contentType": [
                "Article"
            ]
        }

    ]
}
```

In the following example we place the `MediaField_Edit` shape in a card called `Media`, and position the `Media` card first, and the `Content` card second.

``` json

{
    "MediaField_Edit" : [
        {
            "place" : "Parts:0%Media;0",
            "contentType": [
                "Article"
            ]
        }
    ],
    "HtmlField_Edit" : [
        {
            "place": "Parts:0%Content;1",
            "contentType": [
                "Article"
            ]
        }

    ]
}
```

In the following example we place the `MediaField_Edit` shape in a column called `Media`, and position the `Media` column first, and the `Content` column second.
We also specify that the `Content` column will take 9 columns, of the default 12 column grid.

``` json

{
    "MediaField_Edit" : [
        {
            "place" : "Parts:0|Media;0",
            "contentType": [
                "Article"
            ]
        }
    ],
    "HtmlField_Edit" : [
        {
            "place": "Parts:0|Content_9;1",
            "contentType": [
                "Article"
            ]
        }

    ]
}
```

!!! note
    By default the columns will break responsively at the `md` breakpoint, and a modifier will be parsed to `col-md-9`.
    If you want to change the breakpoint, you could also specifiy `Content_lg-9`, which is parsed to `col-lg-9`.
    
### Dynamic part placement

In the following example we place a dynamic part (part without driver, i.e. created in json) `GalleryPart` in a zone called `MyGalleryZone`. When displaying that part inside Content template we would execute:

=== Content-Product.Detail.html
``` html
@await DisplayAsync(Model.MyGalleryZone)
```

Dynamic parts use `ContentPart` shape for detail display with differentiator of Part name, so the placement file would look like this:
``` json
{
  "ContentPart": [
    {
      "place": "MyGalleryZone",
      "differentiator": "GalleryPart"
    }
  ]
}
```

This setup would then show your template (e.g. `GalleryPart.cshtml` or `GalleryPart.Detail.cshtml`) where `DisplayAsync` was called.

If we would like to show the same part in a summary display content template (or any other that isn't `Detail` display type):

=== Content-Product.Summary.html
``` html
@await DisplayAsync(Model.MyGalleryZone)
```

Our placement would look like this (note the `_Summary` suffix to ContentPart name; change your suffix accordingly):

``` json
{
  "ContentPart_Summary": [
    {
      "place": "MyGalleryZone",
      "differentiator": "GalleryPart"
    }
  ]
}
```

This setup would then show your template  (e.g. `GalleryPart.cshtml` or `GalleryPart.Summary.cshtml`) where `DisplayAsync` was called.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/h0lZMQkUApo" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
