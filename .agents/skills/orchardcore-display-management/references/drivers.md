# Display drivers reference

A display driver builds the shapes for a part, field, or model and assigns each a default location. `placement.json` can override those locations later.

## Base classes

| Base | Build context (Display / Edit) | Use |
|------|-------------------------------|-----|
| `ContentPartDisplayDriver<TPart>` | `BuildPartDisplayContext` / `BuildPartEditorContext` | content part |
| `ContentFieldDisplayDriver<TField>` | `BuildFieldDisplayContext` / `BuildFieldEditorContext` | content field |
| `SectionDisplayDriver<TModel, TSection>` | — | settings sections (site/user settings) |
| `DisplayDriver<TModel>` | `BuildDisplayContext` / `BuildEditorContext` | any other model |

## Methods to override

| Method | Runs | Returns |
|--------|------|---------|
| `Display(model, context)` | front-end / admin display | `IDisplayResult` |
| `Edit(model, context)` | render editor | `IDisplayResult` |
| `UpdateAsync(model, context)` | POST — bind & validate | `IDisplayResult` (usually `Edit(...)`) |

All can be sync or async (`DisplayAsync`, `EditAsync`).

## Building a shape

`Initialize<TViewModel>(shapeType, factory)` creates a shape of `shapeType` backed by a populated view model, then chain `.Location(...)`:

```csharp
public override IDisplayResult Display(TextField field, BuildFieldDisplayContext context)
{
    return Initialize<DisplayTextFieldViewModel>(GetDisplayShapeType(context), model =>
    {
        model.Field = field;
        model.Part = context.ContentPart;
        model.PartFieldDefinition = context.PartFieldDefinition;
    })
    .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
    .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
}
```

Other result helpers:
- `View(shapeType, model)` — simple shape from an existing model.
- `Combine(...)` — return several display results from one driver.
- `Dynamic(shapeType)` — build a shape without a strong view model.

## Shape-type helpers

Inside field/part drivers, derive the correct shape type so alternates and placement resolve correctly:

| Helper | Returns |
|--------|---------|
| `GetDisplayShapeType(context)` | display shape type for current display type |
| `GetEditorShapeType(context)` | editor shape type for current editor |

These honor display-mode and editor suffixes (e.g. `TextField_Display__Header`, `TextField_Edit__CustomEditor`).

## Reading the editor (UpdateAsync)

```csharp
public override async Task<IDisplayResult> UpdateAsync(TextField field, UpdateFieldEditorContext context)
{
    await context.Updater.TryUpdateModelAsync(field, Prefix, f => f.Text);

    var settings = context.PartFieldDefinition.GetSettings<TextFieldSettings>();
    if (settings.Required && string.IsNullOrWhiteSpace(field.Text))
    {
        context.Updater.ModelState.AddModelError(Prefix, nameof(field.Text),
            S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
    }

    return Edit(field, context);
}
```

`Prefix` namespaces form fields so multiple instances of the same shape don't collide. Always bind with `Prefix`.

## Location: string vs fluent

```csharp
// String:
.Location("Parameters:5#Settings;1")

// Fluent (equivalent):
.Location(l => l.Zone("Parameters", "5").Tab("Settings", "1"))
```

Fluent builder (`PlacementLocationBuilder`) — start with `.Zone()`, chain in any order, skip levels freely:

| Method | String equivalent |
|--------|-------------------|
| `.Zone("Content", "5")` | `Content:5` |
| `.AsLayoutZone()` | `/` prefix |
| `.Tab("Settings", "1")` | `#Settings;1` |
| `.Card("Details", "2")` | `%Details;2` |
| `.Column("Left", "1", "9")` | `\|Left_9;1` (name, position, width) |
| `.Group("search")` | `@search` |

Full nesting:

```csharp
.Location(l => l
    .Zone("Parameters", "5")
    .Tab("Settings", "1")
    .Card("Details", "2")
    .Column("Left", "3", "9"))
// → "Parameters:5#Settings;1%Details;2|Left_9;3"
```

Per display type:

```csharp
.Location("Summary", l => l.Zone("Content", "1"))
```

## Registration

In the module `Startup.cs`:

```csharp
// Content part driver
services.AddContentPartDisplayDriver<MyPart, MyPartDisplayDriver>();

// Field driver
services.AddContentFieldDisplayDriver<TextField, TextFieldDisplayDriver>();

// Settings section / generic driver
services.AddScoped<IDisplayDriver<ISite>, MySettingsDisplayDriver>();
```

Often paired with the part/field registration:

```csharp
services.AddContentPart<MyPart>()
    .UseDisplayDriver<MyPartDisplayDriver>();
```

## Templates and alternates

The shape type chosen by the driver selects the Razor/Liquid template (`MyPart.cshtml`, `MyPart.Summary.cshtml`). Alternates added via `placement.json` `alternates` or shape metadata let a theme override more specifically. See `src/docs/reference/modules/Templates/README.md` for the alternate naming rules and content-field differentiators.
