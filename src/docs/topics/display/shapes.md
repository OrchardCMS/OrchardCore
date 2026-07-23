# Shapes

Orchard Core builds its user interface from **shapes**. A shape is an object that
contains the data to display and metadata that describes how to display it. Shapes
are assembled into a tree, then each shape is rendered independently into
`IHtmlContent`.

This indirection lets a module provide a default rendering while an application or
theme changes a specific content type, display type, field, or individual shape
without changing the module.

## Create and render a first shape

In a Razor shape template, `New` creates shapes and `DisplayAsync` renders them:

```cshtml
@{
    dynamic car = await New.Car(Brand: "Toyota", Color: "Blue");
}

@await DisplayAsync(car)
```

`New.Car(...)` creates a shape whose type is `Car` and whose properties include
`Brand` and `Color`. Orchard Core looks for a binding for that shape type. The
simplest binding is a Razor file named `Views/Car.cshtml`:

```cshtml
<div>A @Model.Color @Model.Brand car.</div>
```

The same operation in Liquid uses `shape_new`, `shape_add_properties`, and
`shape_render`:

```liquid
{% assign car = "Car" | shape_new %}
{% shape_add_properties car Brand: "Toyota", Color: "Blue" %}
{{ car | shape_render }}
```

From a service, inject `IShapeFactory` and `IDisplayHelper`:

```csharp
dynamic car = await _shapeFactory.CreateAsync("Car");
car.Brand = "Toyota";
car.Color = "Blue";

var html = await _displayHelper.ShapeExecuteAsync((IShape)car);
```

The dynamic `New` property exposed by `IShapeFactory` is shorthand for the same
operation. Both `await New.Car()` and `await New.CarAsync()` create a shape with
the type `Car`.

The [Display Management](../../reference/modules/DisplayManagement/README.md)
reference also describes the `<shape>` tag helper and the equivalent Liquid
filters.

## Understand the shape contract

All shapes implement `IShape`. The default implementation is `Shape`, which is a
dynamic object: assigning an unknown member stores it in the `Properties`
dictionary, and reading that member retrieves the same value.

```csharp
dynamic car = await _shapeFactory.CreateAsync("Car");

car.Brand = "Toyota";

var shape = (IShape)car;
var brand = shape.Properties["Brand"]; // "Toyota"
```

The important members of `IShape` are:

| Member | Purpose |
| --- | --- |
| `Metadata` | Rendering, placement, grouping, alternates, wrappers, and cache information. |
| `Properties` | Data exposed dynamically to templates. |
| `Id`, `TagName`, `Classes`, `Attributes` | Optional hints for generating an HTML element. |
| `Items` | Ordered child items contained by the shape. |
| `AddAsync()` | Adds a child item and its position. |

`Shape` derives from `Composite`, which provides its dynamic property behavior.
`ShapeViewModel` is another `IShape` implementation, but it is not dynamic.

### Shape type and CLR type are independent

`Metadata.Type` is the logical shape type used for binding resolution. It is not
the CLR type of the object. A shape whose logical type is `Car` can be the default
dynamic `Shape`, a custom `IShape` implementation, or a strongly typed model.

For strongly typed templates, `CreateAsync<TModel>()` creates a proxy that derives
from `TModel` and implements `IShape`:

```csharp
public class Car
{
    public string Brand { get; set; }
    public string Color { get; set; }
}
```

```csharp
var shape = await _shapeFactory.CreateAsync<Car>("Car", car =>
{
    car.Brand = "Toyota";
    car.Color = "Blue";
});

var car = (Car)shape;
var metadata = shape.Metadata;
```

The model class must be proxyable: use a non-sealed class with an accessible
parameterless constructor. If the model already implements `IShape`, Orchard Core
creates it directly instead of generating a proxy.

Display drivers normally create strongly typed shapes with `Initialize<TModel>()`,
as shown later in this guide.

### Metadata

Every shape has a `ShapeMetadata` instance. The most commonly used properties are:

| Property | Purpose |
| --- | --- |
| `Type` | Logical shape type used to resolve a binding. |
| `DisplayType` | Display context, such as `Detail`, `Summary`, or `Edit`. Placement and alternate providers can use it. |
| `Name` | Name of this shape inside its parent. |
| `Differentiator` | Distinguishes multiple uses of the same shape type, such as two fields. |
| `Position` | Ordering position inside the parent shape. |
| `Prefix` | HTML field prefix used while rendering editors. |
| `Alternates` | More specific binding names, ordered from lowest to highest priority. |
| `Wrappers` | Shape types rendered around this shape. |
| `TabGrouping`, `CardGrouping`, `ColumnGrouping` | Grouping information used by content zones. |
| `ChildContent` | The current rendered content of the shape. |

`DisplayType` does not by itself change the template name. Display drivers and
shape table providers use it to add display-type alternates and to select
placement rules.

### HTML properties

The `Id`, `TagName`, `Classes`, and `Attributes` members let a shape carry HTML
hints without carrying HTML. `GetTagBuilder()` turns these hints into a
`TagBuilder`:

```csharp
var shape = await _shapeFactory.CreateAsync("Car");
shape.Id = "featured-car";
shape.TagName = "article";
shape.Classes.Add("car");
shape.Classes.Add("car--featured");
shape.Attributes["data-brand"] = "Toyota";
```

The template can use those values consistently:

```cshtml
@using OrchardCore.DisplayManagement
@{
    var shape = (IShape)Model;
    var tagBuilder = shape.GetTagBuilder("div");
}

@tagBuilder.RenderStartTag()
A @Model.Color @Model.Brand car.
@tagBuilder.RenderEndTag()
```

`GetTagBuilder()` defaults to a `span` when neither `TagName` nor a default tag
name is supplied.

## Follow the rendering pipeline

Rendering `@await DisplayAsync(shape)` invokes `IDisplayHelper`, which delegates
to `IHtmlDisplay`. The default pipeline performs these steps:

1. Ignore a `null` shape or an empty deferred zone.
2. Run global, descriptor, and shape-instance `Displaying` handlers.
3. When `ChildContent` was not already supplied by a cache, run descriptor
   `Processing` handlers and resolve the first available binding by checking
   alternates from highest to lowest priority, then the shape type and its
   fallback types.
4. Run shape-instance `Processing` handlers and execute the binding.
5. Render each wrapper around the current `ChildContent`.
6. Run `Displayed` handlers and return the final `IHtmlContent`.
7. Run global finalization handlers even if rendering throws.

If no binding can be resolved, rendering throws an `InvalidOperationException`
that names the missing shape type and active theme. There is no implicit
catch-all HTML rendering for a typed shape.

### Bindings, descriptors, and the shape table

A **shape binding** is the delegate that turns one shape into `IHtmlContent`. It
can come from:

- A Razor template.
- A Liquid template.
- A method marked with `[Shape]`.
- A dynamic `IShapeBindingResolver`.

A **shape descriptor** stores configuration for a fundamental shape type:
creation and display handlers, bindings, default placement, and wrappers.

The **shape table** contains the descriptors and bindings available for the
current tenant and active theme. `IShapeTableProvider` implementations build and
alter that table. The startup application has the highest override priority,
followed by the active theme and its base themes, then modules in dependency
order.

For normal customization, add a template to the active theme rather than working
with the shape table directly.

## Name templates and shape types correctly

Templates can be placed in these directories:

- `Views`
- `Views/Items`
- `Views/Parts`
- `Views/Fields`
- `Views/Elements`

Templates below `Views/Parts`, `Views/Fields`, and `Views/Elements` automatically
receive the corresponding `Parts_`, `Fields_`, or `Elements_` prefix.

Template filenames use punctuation that is converted to canonical shape names:

- A hyphen (`-`) becomes a double underscore (`__`). This is a **breaking
  separator** that creates fallback levels.
- A dot (`.`) becomes a single underscore (`_`). This is a **non-breaking
  separator**.
- When the final dot appears after the final hyphen, the suffix is interpreted as
  a display type and inserted before the first `__`.

| Canonical binding name | Razor or Liquid filename |
| --- | --- |
| `Car` | `Car.cshtml` or `Car.liquid` |
| `Car_Color__Blue` | `Car.Color-Blue.cshtml` or `Car.Color-Blue.liquid` |
| `Content__BlogPost` | `Content-BlogPost.cshtml` or `Content-BlogPost.liquid` |
| `Content_Summary__BlogPost` | `Content-BlogPost.Summary.cshtml` or `Content-BlogPost.Summary.liquid` |
| `Fields_Common_Text__FirstName` | `Views/Fields/Common.Text-FirstName.cshtml` or `.liquid` |

For example, `Content-BlogPost.Summary.cshtml` is harvested as the binding
`Content_Summary__BlogPost`, not `Content__BlogPost_Summary`.

!!! note
    Use canonical names such as `Content_Summary__BlogPost` in C# APIs,
    `placement.json`, and shape metadata. Use the filename form such as
    `Content-BlogPost.Summary.cshtml` only for template files.

See [Templates](../../reference/modules/Templates/README.md) for the complete
list of content, part, field, widget, and editor alternates.

## Specialize rendering with alternates

An alternate is another fully qualified binding name that may render a shape.
Alternates are useful when the shape type is reusable but one context needs a
more specific template.

```csharp
var shape = await _shapeFactory.CreateAsync<Car>("Car", car =>
{
    car.Brand = "Toyota";
    car.Color = "Blue";
});

shape.Metadata.Alternates.Add("Car_Color__Blue");
shape.Metadata.Alternates.Add("Car_Brand__Toyota");
```

Alternates added later have higher priority. The resolver therefore checks:

1. `Car_Brand__Toyota`
2. `Car_Color__Blue`
3. `Car`

The matching template filenames are:

1. `Car.Brand-Toyota.cshtml`
2. `Car.Color-Blue.cshtml`
3. `Car.cshtml`

Do not add the base type itself as an alternate; the resolver checks it after all
alternates.

Shape types containing `__` also fall back by removing the last segment. For
example, a type or alternate named `Car__Toyota__Hybrid` falls back through:

1. `Car__Toyota__Hybrid`
2. `Car__Toyota`
3. `Car`

This fallback is separate from the explicit `Alternates` collection.

Orchard Core adds many alternates automatically. Content shapes, for example, can
receive alternates for their content type, display type, alias, slug, and other
context. Prefer those standard alternates to conditionals inside a shared
template.

### Add alternates consistently with a shape table provider

When shapes of the same type are created in multiple places, an
`IShapeTableProvider` centralizes their conventions:

```csharp
public sealed class CarShapeTableProvider : IShapeTableProvider
{
    public ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Car")
            .OnDisplaying(context =>
            {
                if (context.Shape is Car car)
                {
                    context.Shape.Metadata.Alternates.Add($"Car_Color__{car.Color}");
                    context.Shape.Metadata.Alternates.Add($"Car_Brand__{car.Brand}");
                }
            });

        return ValueTask.CompletedTask;
    }
}
```

Register it with the feature:

```csharp
services.AddShapeTableProvider<CarShapeTableProvider>();
```

`OnDisplaying` runs before binding resolution, so it can derive alternates from
initialized shape data. Values used in binding names should be controlled,
canonical identifiers rather than arbitrary user input.

## Wrap a shape

Wrappers render around a shape without replacing its selected template:

```csharp
shape.Metadata.Wrappers.Add("Panel");
```

`Views/Panel.cshtml` receives the same shape. Its
`Model.Metadata.ChildContent` contains the output rendered so far:

```cshtml
<section class="panel">
    @Model.Metadata.ChildContent
</section>
```

Wrappers are processed in insertion order. The first wrapper added is the
innermost; each later wrapper surrounds the previous result.

Alternates and wrappers solve different problems:

| Feature | Effect |
| --- | --- |
| Alternate | Selects a different binding for the shape itself. |
| Wrapper | Renders another binding around the selected shape output. |

Both can be added from code, a metadata tag helper, or `placement.json`.

## Compose shapes with items

A shape can contain ordered child items. `AddAsync()` accepts shapes,
`IHtmlContent`, strings, and other objects that can be wrapped as positioned
items:

```csharp
var garage = await _shapeFactory.CreateAsync("Garage");
var firstCar = await _shapeFactory.CreateAsync("Car");
var secondCar = await _shapeFactory.CreateAsync("Car");

await garage.AddAsync(secondCar, "2");
await garage.AddAsync(firstCar, "1");
```

The `Items` collection is sorted lazily when it is read. Positions use numeric,
hierarchical ordering rather than string ordering: `1`, `1.1`, `2`, and `10`
appear in that order. `before` and `after` place items at the beginning and end,
and an empty position is treated as `0`.

A container binding normally renders each item:

```cshtml
@foreach (var item in Model.Items)
{
    @await DisplayAsync(item)
}
```

The built-in `Zone` binding follows this pattern.

## Understand layouts and zones

Many important shapes are `IZoneHolding` shapes. A zone-holding shape exposes
named child containers through its `Zones` property:

```text
Layout
|-- Header
|   |-- Branding
|   `-- Menu
|-- Content
|   `-- Content
|       |-- TitlePart
|       `-- HtmlBodyPart
`-- Footer
    `-- Copyright
```

`ILayoutAccessor.GetLayoutAsync()` gets or creates the request's `Layout` shape.
Layout zones are regular shapes with the type `Zone`.

```csharp
dynamic layout = await _layoutAccessor.GetLayoutAsync();
var menu = await _shapeFactory.CreateAsync("Menu");

await layout.Header.AddAsync(menu, "5");
```

Zones are created on demand. Accessing a missing zone returns a null-like
`ZoneOnDemand`; adding the first non-null item creates the real zone and stores it
on the parent.

```cshtml
@if (Model.Zones["Header"] != null)
{
    @await DisplayAsync(Model.Zones["Header"])
}
```

`Zones.IsNotEmpty("Header")` and the `IShape.IsNullOrEmpty()` extension method are
strongly typed alternatives for checking whether a zone has materialized.

The content shapes built by display managers also hold zones, but those child
zones use the type `ContentZone`. A `ContentZone` renders items directly unless
one or more children have tab, card, or column grouping metadata. When grouping
is present, it builds the corresponding grouping container shapes in this order:
tabs, then cards, then columns.

The active theme defines which layout zones are meaningful. Placement determines
which generated shapes are added to those zones.

## Produce shapes with display drivers

Most content, content part, and content field shapes come from display drivers.
A driver returns an `IDisplayResult`; `ShapeResult` is the result that lazily
creates, configures, and places one shape.

```csharp
public sealed class ProductPartDisplayDriver
    : ContentPartDisplayDriver<ProductPart>
{
    public override IDisplayResult Display(
        ProductPart part,
        BuildPartDisplayContext context)
    {
        return Initialize<ProductViewModel>("ProductPart", model =>
        {
            model.Title = part.Title;
            model.Price = part.Price;
        })
        .Location("Detail", "Content:5")
        .Location("Summary", "Content:1");
    }
}
```

`Initialize<TModel>()` creates a strongly typed shape. The string
`ProductPart` is its logical shape type; `ProductViewModel` is its CLR base type.
The locations are driver defaults for the `Detail` and `Summary` display types.

Drivers can use:

| Method | Purpose |
| --- | --- |
| `Initialize<TModel>()` | Create and initialize a strongly typed shape. |
| `Dynamic()` | Create and initialize a dynamic `Shape`. |
| `Copy()` | Copy public properties from an object to a dynamic shape. |
| `View()` | Wrap an existing model in `ShapeViewModel<TModel>`. |
| `Shape()` | Return an existing `IShape`. |
| `Factory()` | Create a shape lazily with a custom factory. |
| `Combine()` | Return multiple display results. |

Before creating the shape, `ShapeResult` resolves placement. A hidden or
non-matching result is therefore not created. When it is created, `ShapeResult`
sets its name, differentiator, display type, prefix, grouping, alternates,
wrappers, and position, then adds it to the selected zone.

### Differentiators

A differentiator identifies one use of a reusable shape type. For example, all
text fields may use the `TextField` shape type, while differentiators such as
`Article-Subtitle` and `Article-Description` let placement target each field
independently.

Content part and field driver base classes set their standard differentiators.
Custom display results can call `.Differentiator("value")`.

## Control location with placement

Driver locations are defaults. A module, theme, or startup application can
override them with `placement.json`:

```json
{
  "ProductPart": [
    {
      "place": "Content:3",
      "displayType": "Detail",
      "alternates": [ "ProductPart_Featured" ],
      "wrappers": [ "Panel" ]
    },
    {
      "place": "-",
      "displayType": "Summary"
    }
  ]
}
```

The first rule moves the detail shape, adds an alternate, and wraps its output.
The second rule hides the summary shape. Rules can also filter by differentiator,
content type, content part, and request path.

Placement can substitute the logical shape type:

```json
{
  "ProductPart": [
    {
      "place": "Content:3",
      "shape": "ProductCard"
    }
  ]
}
```

This changes `Metadata.Type` to `ProductCard`. It clears the shape's existing
alternates and wrappers before applying any alternates and wrappers from the
matching placement rule. The object itself is not converted to another CLR type.

See [Placement](../../reference/modules/Placement/README.md) for its complete
syntax, precedence, position rules, grouping, and differentiator patterns.

## Morph a shape while rendering

Razor's `DisplayAsAsync()` renders an existing shape as another logical type:

```cshtml
@await DisplayAsAsync(Model, "CompactCar")
```

It clears the shape's alternates by default, assigns the new
`Metadata.Type`, clears `ChildContent`, and renders the same shape instance again.
Pass `clearAlternates: false` to preserve its alternates:

```cshtml
@await DisplayAsAsync(Model, "CompactCar", clearAlternates: false)
```

Because this operation changes the shape instance, later renders use the new
type. Use it when the new binding expects the same data contract. Creating a new
shape is clearer when the child has different data or an independent lifecycle.

Shape morphing at render time and placement's `shape` substitution both change
the logical shape type; neither changes the CLR object.

## Define a binding in code

When a small rendering function is more appropriate than a file, implement
`IShapeAttributeProvider` and mark a method with `[Shape]`:

```csharp
public sealed class CarShapes : IShapeAttributeProvider
{
    [Shape("Car")]
    public IHtmlContent Car(IShape Shape)
    {
        var car = (Car)Shape;
        var tagBuilder = Shape.GetTagBuilder("div");
        tagBuilder.InnerHtml.Append($"A {car.Color} {car.Brand} car.");

        return tagBuilder;
    }
}
```

Register the provider:

```csharp
services.AddShapeAttributes<CarShapes>();
```

When `[Shape]` has no argument, the method name is the shape type. Method
parameters can receive shape data and display services through the shape binding
parameter binder. The returned value can be `IHtmlContent`,
`Task<IHtmlContent>`, or a value that can be converted to HTML content.

An `IShapeTableProvider` can also define a binding with `BoundAs()`. Attribute
providers are usually simpler for code-rendered shapes, while shape table
providers are useful when creation, placement, alternates, wrappers, and
lifecycle handlers must be configured together.

## Use lifecycle hooks carefully

Orchard Core exposes hooks at several scopes:

| Scope | API |
| --- | --- |
| One shape instance | `ShapeMetadata.OnDisplaying()`, `OnProcessing()`, and `OnDisplayed()` |
| One display result | `ShapeResult.Displaying()` and `Processing()` |
| One shape type | `IShapeTableProvider` with `OnCreating()`, `OnCreated()`, `OnDisplaying()`, `OnProcessing()`, and `OnDisplayed()` |
| Every rendered shape | `IShapeDisplayEvents` |
| Every created shape | `IShapeFactoryEvents` |

Use `Displaying` to add alternates or attributes that are needed for binding
resolution. Use `Processing` to load state only when the binding will actually
render rather than use cached child content. Use `Displayed` to inspect or replace
the rendered `ChildContent`.

`IShapeDisplayEvents.DisplayingFinalizedAsync()` runs from a `finally` block and
is intended for cleanup that must happen after every attempted render.

### `ChildContent`

`ShapeMetadata.ChildContent` belongs to the rendering pipeline:

- A binding assigns the initial rendered content.
- Each wrapper reads the current value and replaces it with wrapped content.
- A cache can supply it before binding resolution, which skips binding execution.
- Displayed handlers can replace it.

Do not use `ChildContent` as the shape's input data. Put input values in strongly
typed model properties or `IShape.Properties`.

## Diagnose binding and placement problems

Enable **Settings** > **Debugging** > **Write shape debug information** to add
comments around rendered shapes:

```html
<!--shape-start type:Menu bindings:Menu__Main => Themes/Contoso/Views/Menu-Main.cshtml (razor) -->
...
<!--shape-end type:Menu-->
```

The comments show the logical type and the Razor, Liquid, or code binding that
rendered it. They are the fastest way to confirm which alternate won.

When an override is not selected, check:

1. The shape's `Metadata.Type`, `DisplayType`, and ordered `Alternates`.
2. Whether code uses a canonical binding name while the file uses the filename
   form.
3. Whether the template is under a harvested `Views` directory.
4. Whether a later-added alternate has higher priority.
5. Whether placement changed the shape type or cleared alternates and wrappers.
6. Whether the active theme or startup application overrides the module binding.

For location problems, check the shape type and differentiator against the
matching `placement.json` rule.

## Summary

A shape is an `IShape` carrying display data, ordered child items, HTML hints, and
rendering metadata. `IShapeFactory` creates it, display drivers and placement
compose it into zones, and `IDisplayHelper` resolves its most specific available
binding. Alternates replace a binding, wrappers surround its output, and shape
type substitution or morphing reuses the same object under another logical type.

The central naming rule is:

- Use canonical names with `_` and `__` in APIs and metadata.
- Use dots and hyphens in Razor and Liquid filenames.
- Add alternates from least to most specific because the last alternate wins.

## Related topics

- [Customize the Display](README.md)
- [Display Management](../../reference/modules/DisplayManagement/README.md)
- [Templates and Alternates](../../reference/modules/Templates/README.md)
- [Placement](../../reference/modules/Placement/README.md)
- [Liquid](../../reference/modules/Liquid/README.md)
