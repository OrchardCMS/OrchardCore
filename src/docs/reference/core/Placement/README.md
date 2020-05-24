# OrchardCore.DisplayManagement

This article is about Display management and placement files.

## Placement files

Any module or theme can contain an optional `placement.json` file providing custom placement logic.

!!! note
    The `placement.json` file must be added at the root of a module or a theme.

### Format

A `placement.json` file contains an object whose properties are shape names. Each of these properties is an array of placement rules.

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

- Their original type, which is the property name of the placement rule, like `TextField`.
- `displayType` (Optional): The display type, like `Summary` and `Detail` for the most common ones.
- `differentiator` (Optional): The differentiator which is used to distinguish shape types that are reused for multiple elements, like field names.

Additional custom filter providers can be added by implementing `IPlacementNodeFilterProvider`.

For shapes that are built from a content item, you can filter by the following built in filter providers:

- `contentType` (Optional): A single ContentType or an array of ContentTypes that content item from which the shape was built should match.
- `contentPart` (Optional): A single ContentPart or an of array of ContentParts that content item from which the shape was built should contain.
- `path` (Optional): A single path or an of array of paths that should match the request path.

Placement information consists of:

- `place` (Optional): The actual location of the shape in the rendered zone. A value of "-" will hide the shape.
- `alternates` (Optional): An array of alternate shape types to add to the current shape's metadata.
- `wrappers` (Optional): An array of shape types to use as wrappers for the current shape.
- `shape` (Optional): A substitution shape type.

```json
{
  "TextField": [
    {
    "displayType": "Detail",
    "differentiator": "Article-MyTextField",
        "contentType": ["Page", "BlogPost"],
        "contentPart": ["HtmlBodyPart"],
        "path": ["/mypage"],

    "place": "Content",
    "alternates": [ "TextField_Title" ],
    "wrappers": [ "TextField_Title" ],
    "shape": "AnotherShape"
    }
  ],
}
```

### Placing Fields

Fields have a custom differentiator as their shape is used in many places.  
It is built using the `Part` it's contained in, and the name of the field.  
For instance, if a field named `MyField` would be added to an `Article` content type, its differentiator would be `Article-MyField`.  
If a field named `City` was added to an `Address` part then its differentiator would be `Address-City`.

## Shapes

### What is a shape?

Everything you need to know about Shapes is in [this video](https://youtu.be/gKLjtCIs4GU).

### Rendering a shape

You can use the `<shape>` tag helper to render any shape, even pass properties.

``` html tab="Razor"
@{
    var intValue = 1;
    var stringValue = "a";
}

@await DisplayAsync(await New.MyShape(Foo: 1, Bar: "a"))

<shape type="MyShape" foo="1" bar="a" />

<shape type="MyShape" prop-foo="1" bar="a" />

<shape type="MyShape" prop-foo="@intValue" prop-bar="@stringValue" />
```

``` liquid tab="Liquid"
{% assign customShape = "MyShape" | shape_new %}
{% shape_add_properties customShape my_string: "String Test 3", my_int: 1 %}
{{ customShape | shape_render }}

{% "MyShape" | shape_new | shape_properties: my_int: 3, my_string: "String Test 3" | shape_render %}
```

For rendering content items, you could also use the following tag helper.
Note: you need to add `@addTagHelper *, OrchardCore.Contents` to your `_ViewImports.cshtml` file to load this tag helper.

``` html tab="Razor"
<contentitem alias="alias:main-menu" display-type="Detail" />
```

``` liquid tab="Liquid"
{% contentitem alias:"alias:main-menu" display_type="Detail" %}
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

## Shape differentiators

You can find information about shape differentiators in the [Templates documentation](../../modules/Templates/README.md#content-field-differentiator)

## Tabs

If you want to place a part in a different tab, you use the `#` character in order to specify the tab in which it will be rendered.

```json
{
  "TitlePart_Edit": [
    {
      "place": "Parts#MyTab:0"
    }
  ]
}
```
