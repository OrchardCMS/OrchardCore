# OrchardCore.DisplayManagement

## Shapes

### What is a shape?

Please review the following videos to enhance your understanding of shapes:

- [Understanding Shapes](https://youtu.be/gKLjtCIs4GU)
- [Demystifying Shapes](https://www.youtube.com/watch?v=yaZhKuD2qoI)

These videos will provide valuable insights and a clear explanation of various shapes and their properties.

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

#### Adding properties with additional tag helpers

Properties can be passed to a shape by adding attributes to the shape tag helper, as mentioned above. But you can also use the `<add-property>` tag helper inside `<shape>`. This even lets you pass Razor code as properties with the `IHtmlContent` value, if you omit the `value` attribute. Something that can't be easily done otherwise.

```xml
<shape type="MyShape">
    <add-property name="foo" value="1" />
    <add-property name="bar" value="a" />
    <add-property name="content">
        <h2>Some complicated HTML</h2>
        <div>
            You can even include shapes:
            <shape type="AnotherShape" prop-count="10" />
        </div>
    </add-property>
</shape>
```

This is the same as `<shape type="MyShape" pro-foo="1" prop-bar="a" prop-content="@someHtmlContentVariable" />` where you'd have to construct `someHtmlContentVariable` separately. Of course, you can mix and match the different formats, for example, to only use `<add-property>` when you want to pass HTML content as property.

### Date Time shapes

#### `DateTime`

Renders a `Date` and `Time` value using the TimeZone of the request.

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

#### `Duration`

Renders a duration value using the given TimeSpan.

| Parameter | Type | Description |
| --------- | ---- |------------ |
| `timeSpan` | `TimeSpan?` | The time span to render. |

Tag helper example:

```html
<duration timeSpan="@TimeSpan.FromSeconds(5)" />
```

### Related Articles
- [Placement](../Placement/README.md)

