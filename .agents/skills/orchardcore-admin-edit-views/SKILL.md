---
name: orchardcore-admin-edit-views
description: Creates and updates OrchardCore admin edit views using the ocat-* CSS class conventions, grouping their fields in cards declared by placement or ocat-card markup. Use when creating or modifying Edit view files (*.Edit.cshtml, *.Fields.Edit.cshtml) or the display drivers of admin editors, to ensure correct layout and styling.
status: completed
---

# OrchardCore Admin Edit Views

This skill guides you through creating and updating admin edit view files using the `ocat-*` CSS class conventions required by the OrchardCore admin theme.

## The `ocat-*` CSS Class System

The `ocat-*` CSS classes provide a standardized two-column layout for admin form fields. Every form field uses a consistent structure:

- **Left column** – The field label (`ocat-label`)
- **Right column** – The input, validation message, and hint (`ocat-end`)

### Core Classes

| Class | Element | Purpose |
|-------|---------|---------|
| `ocat-wrapper` | `<div>` | Outer container for a form field row (replaces `mb-3`/`form-group`) |
| `ocat-label` | `<label>` | Styles the field label in the left column (replaces `form-label`) |
| `ocat-label-required` | `<label>` | Add alongside `ocat-label` to mark a field as required |
| `ocat-end` | `<div>` | Wraps the input, validation, and hint in the right column |
| `ocat-end-offset` | `<div>` | Right-column content with no left label (e.g., checkboxes) |
| `ocat-limited-wrapper` | `<div>` | Outer row for limited-width controls such as short text, numbers, selects, or small settings inputs |
| `ocat-limited` | `<div>` | Constrains the control width inside `ocat-limited-wrapper` |

## Choosing the Right Wrapper

Use this decision guide when creating or updating admin edit views:

| Scenario | Preferred Pattern |
|----------|-------------------|
| Standard full-width field row | `ocat-wrapper` + `ocat-label` + `ocat-end` |
| Standalone checkbox/toggle row with no left label | `ocat-wrapper` + `ocat-end-offset` |
| Standalone alert or notice in the form flow | `ocat-wrapper` + `ocat-end-offset` |
| A group of fields (a section of the form) | A card, see [Grouping Fields in Cards](#grouping-fields-in-cards) |
| Top-level limited-width field row (small text, number, select, token, path, port, etc.) | `ocat-limited-wrapper` + `ocat-label` + `ocat-limited` |
| Content part / content field editor that already needs wrapper classes like `field-wrapper-*` or `content-part-wrapper-*` | Keep the outer `ocat-wrapper`, then place `ocat-limited-wrapper` inside `ocat-end` |

### Wrapper Selection Rules

1. Use `ocat-wrapper` for the default admin edit-view row structure.
2. Use `ocat-limited-wrapper` when the field should stay visually narrow instead of stretching the full editor width.
3. If the outer row also needs Orchard-specific wrapper classes such as `field-wrapper-*` or `content-part-wrapper-*`, do **not** replace that outer row with `ocat-limited-wrapper`; keep the outer `ocat-wrapper` and nest `ocat-limited-wrapper` inside `ocat-end`.
4. Do **not** use legacy helpers or Bootstrap-only layout classes like `mb-3`, `form-group`, `form-label`, `@Orchard.GetWrapperClasses()`, or `@Orchard.GetLimitedWidthWrapperClasses()`.
5. Alerts, notices, `<legend>`, and section headings that are part of the edit form but have no left-column label should use their own `ocat-wrapper` + `ocat-end-offset` row.
6. A single row should use **either** `ocat-end` **or** `ocat-end-offset`, never both.

### Standard Field Structure

```html
<div class="ocat-wrapper" asp-validation-class-for="FieldName">
    <label asp-for="FieldName" class="ocat-label">@T["Label Text"]</label>
    <div class="ocat-end">
        <input asp-for="FieldName" class="form-control" />
        <span asp-validation-for="FieldName"></span>
        <span class="hint">@T["Descriptive hint text."]</span>
    </div>
</div>
```

## Grouping Fields in Cards

Every admin form groups its fields in cards, so that a form reads as a few logical sections, e.g. "Server", "Authentication" and "Sender" for the SMTP settings, instead of one long column of fields. A card has a short title, a noun phrase in sentence case, and optionally a one-line description.

Every card comes from placement: views never write card markup.

### Screens built by display drivers: cards come from placement

Screens composed of the shapes of display drivers (site settings, users, roles, content type, part and field settings, workflow activities, deployment steps, admin menu nodes, sitemap sources, rule conditions, ...) render the shapes in a content zone, which groups the shapes placed in the same card. Declare the card in the placement location of the shape, never with markup in its view:

```csharp
internal readonly IStringLocalizer S;

public SmtpSettingsDisplayDriver(IStringLocalizer<SmtpSettingsDisplayDriver> stringLocalizer)
{
    S = stringLocalizer;
}

public override IDisplayResult Edit(ISite site, SmtpSettings settings, BuildEditorContext context)
{
    return Combine(
        Initialize<SmtpSettingsViewModel>("SmtpSettingsServer_Edit", model => ...)
            .Location(l => l.Zone("Content", "1").Card(S["Server"], "10"))
            .OnGroup(GroupId),
        Initialize<SmtpSettingsViewModel>("SmtpSettingsAuthentication_Edit", model => ...)
            .Location(l => l.Zone("Content", "2").Card(S["Authentication"], "20"))
            .OnGroup(GroupId));
}
```

- Localize the card name with the `S` localizer of the driver: the name is the title of the card.
- The shapes placed in the same card, from any driver, share it, ordered by their zone position. The card position orders the cards.
- A view holding several logical sections is split into one shape per card, each with its own view, bound to the same view model and prefix, so `UpdateAsync` keeps binding the model once. For instance the general settings render `Settings_Edit__Site` (`Settings-Site.Edit.cshtml`), `Settings_Edit__TimeZone` and `Settings_Edit__Pagination` in the "General", "Regional settings" and "Pagination" cards.
- Cards combine with tabs: `.Location(l => l.Zone("Content", "1").Tab("Site", "10").Card(S["General"], "10"))` renders the card in the tab.
- A `placement.json` file moves a shape to another card with the `%` delimiter, e.g. `"place": "Content:5%Server;10"`. Card names written in placement files aren't localized.
- The views of these shapes only contain their rows: no card markup, and no heading row, the title of the card replaces it.
- A single shape still declares its card: a screen made of one card is consistent with the others.

#### Content item editors

Each content part of a content item is placed in a card titled with its display name (`Parts:5%Html Body`, see `ContentTypePartDefinitionEditorExtensions.GetEditorCardName`), and `ContentPart.Edit.cshtml` renders the editors of the part and of its fields in it, with the description of the part. The editor shapes of content parts and content fields don't declare cards.

- A field placed in a card of its part, e.g. `"place": "Parts.Product:5%Shipping"`, is grouped in that card inside the card of the part.
- A part placed in another card of the parts zone, e.g. `"ContentPart_Edit": [{ "differentiator": "Product-AliasPart", "place": "Parts:5%Metadata" }]`, shares that card with the other parts placed in it, as a section titled with its display name.
- A part placing shapes in the parts zone itself, such as the SEO part in its own tab, declares their cards in their placement, as any driver.

### Admin pages with a form: an editor built by display drivers

A page of a controller that edits a view model (e.g. creating a template, editing an OpenID application) doesn't render the fields itself: it renders an editor built by the display drivers of its view model, so its cards come from placement too, and other modules can add fields or cards to it. `OrchardCore.Templates` (`TemplateController`, `TemplateDisplayDriver`) is the reference.

1. **The view model** carries the editor: `[BindNever] public IShape Editor { get; set; }`.
2. **A display driver** of the view model renders one shape per card, each a copy of the view model, and binds the fields of its shapes. It keeps the names the page posts (no `TemplateViewModel.` prefix), so the controller, its validation keys and the scripts of the page keep working:

```csharp
internal sealed class TemplateDisplayDriver : DisplayDriver<TemplateViewModel>
{
    internal readonly IStringLocalizer S;

    public TemplateDisplayDriver(IStringLocalizer<TemplateDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(TemplateViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<TemplateViewModel>("TemplateGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<TemplateViewModel>("TemplateContent_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(S["Template"], "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(TemplateViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.Description, m => m.Content);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(TemplateViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(TemplateViewModel source, TemplateViewModel model)
    {
        model.Name = source.Name;
        // ...
    }
}
```

   Register it with `services.AddDisplayDriver<TemplateViewModel, TemplateDisplayDriver>()`. Each shape has its own view (`TemplateGeneral.Edit.cshtml`, `@model TemplateViewModel`) holding only its rows. Use `Initialize<T>`, not `View(shapeType, model)`: the latter wraps the model in a `ShapeViewModel<T>`, which renames the fields `Value.Name`.
3. **The root template** of the editor, named after the view model, renders its zones: `TemplateViewModel.Edit.cshtml` holds `@await DisplayAsync(Model.Content)`.
4. **The controller** injects `IDisplayManager<TemplateViewModel>` and `IUpdateModelAccessor`. Its GET actions set `model.Editor = await _displayManager.BuildEditorAsync(model, _updateModelAccessor.ModelUpdater, isNew)`, and its POST actions, once the model is bound, `model.Editor = await _displayManager.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, isNew)` before validating and saving as before. Every path returning the view sets the editor.
5. **The page view** keeps the form, its hidden inputs, its buttons and the scripts of the page, and renders the fields with `@await DisplayAsync(Model.Editor)`.

### How cards render

The admin theme renders a card as a `<fieldset class="ocat-card">` titled by its `<legend class="ocat-card-title">`, always expanded: cards group fields, they don't hide them, so never group fields in an accordion, a collapse or a `<details>` element. Views never write card markup themselves.

- In a tab, which already frames them, the cards render as fieldsets titled by their legends, without the box of a card. The only card of a tab, or a card named as its tab, renders its fields directly, as its title would repeat the tab.
- A card inside a card renders as a section.
- The buttons of the form (Save, Cancel) stay after the editor. Tables and lists belonging to a group, e.g. the permissions of a role, are shapes of their card. Modal dialogs, list filters and inline editors don't use cards.

| Class | Element | Purpose |
|-------|---------|---------|
| `ocat-card` | `<fieldset>` | A group of fields. Being a fieldset, screen readers announce its title as the name of the group |
| `ocat-card-title` | `<legend>` | The title of the card |
| `ocat-card-body` | `<div>` | The rows of the card |
| `ocat-card-description` | `<p>` | A description, e.g. of a content part, at the top of its card |
| `ocat-card-section`, `ocat-card-section-title` | `<div>` | A titled section in a card, e.g. a part sharing a card with other parts |

## Field Type Templates

### Text Input

```html
<div class="ocat-wrapper" asp-validation-class-for="FieldName">
    <label asp-for="FieldName" class="ocat-label">@T["Label"]</label>
    <div class="ocat-end">
        <input asp-for="FieldName" class="form-control" />
        <span asp-validation-for="FieldName"></span>
        <span class="hint">@T["Hint text."]</span>
    </div>
</div>
```

### Required Text Input

Every field whose editor rejects an empty value (`[Required]`, a model error on empty, or an HTML `required` attribute) gets `ocat-label-required` on its label. Add it conditionally when the field is only required in some states, e.g. `@(settings.Required ? "ocat-label-required" : null)`, and don't add it to fields whose empty value is accepted or generated.

```html
<div class="ocat-wrapper" asp-validation-class-for="FieldName">
    <label asp-for="FieldName" class="ocat-label ocat-label-required">@T["Label"]</label>
    <div class="ocat-end">
        <input asp-for="FieldName" class="form-control" />
        <span asp-validation-for="FieldName"></span>
        <span class="hint">@T["Hint text."]</span>
    </div>
</div>
```

### Code/Script Input

```html
<div class="ocat-wrapper" asp-validation-class-for="Expression">
    <label asp-for="Expression" class="ocat-label">@T["Expression"]</label>
    <div class="ocat-end">
        <input type="text" asp-for="Expression" class="form-control code" />
        <span asp-validation-for="Expression"></span>
        <span class="hint">@T["A JavaScript expression. Example: {0}", "input(\"Count\") > 0"]</span>
    </div>
</div>
```

### Textarea

```html
<div class="ocat-wrapper" asp-validation-class-for="Content">
    <label asp-for="Content" class="ocat-label">@T["Content"]</label>
    <div class="ocat-end">
        <textarea asp-for="Content" class="form-control" rows="5"></textarea>
        <span asp-validation-for="Content"></span>
        <span class="hint">@T["Hint text."]</span>
    </div>
</div>
```

### Select / Dropdown

```html
<div class="ocat-wrapper" asp-validation-class-for="FieldName">
    <label asp-for="FieldName" class="ocat-label">@T["Label"]</label>
    <div class="ocat-end">
        <select asp-for="FieldName" class="form-select">
            <option value="Option1">@T["Option 1"]</option>
            <option value="Option2">@T["Option 2"]</option>
        </select>
        <span asp-validation-for="FieldName"></span>
        <span class="hint">@T["Hint text."]</span>
    </div>
</div>
```

### Checkbox (No Left Label)

Use `ocat-end-offset` when there is no label in the left column (e.g., for checkboxes):

```html
<div class="ocat-wrapper" asp-validation-class-for="IsEnabled">
    <div class="ocat-end-offset">
        <div class="form-check">
            <input type="checkbox" class="form-check-input" asp-for="IsEnabled" />
            <label asp-for="IsEnabled" class="form-check-label">@T["Enable feature"]</label>
        </div>
        <span class="hint dashed">@T["Check to enable this feature."]</span>
    </div>
</div>
```

### Headline / Alert Row (No Left Label)

Use this for standalone form guidance or warnings. To start a new section of a form, use a card instead of a heading row (see [Grouping Fields in Cards](#grouping-fields-in-cards)):

```html
<div class="ocat-wrapper">
    <div class="ocat-end-offset">
        <div class="alert alert-warning" role="alert">
            @T["Important information for editors."]
        </div>
    </div>
</div>
```

### Input Group (with prefix/suffix)

```html
<div class="ocat-wrapper" asp-validation-class-for="Url">
    <label asp-for="Url" class="ocat-label">@T["URL"]</label>
    <div class="ocat-end">
        <div class="input-group">
            <span class="input-group-text">https://</span>
            <input asp-for="Url" type="text" class="form-control" />
        </div>
        <span asp-validation-for="Url"></span>
        <span class="hint">@T["The full URL."]</span>
    </div>
</div>
```

### Limited-Width Field Row

Use this when the whole row is a compact admin field, such as a page size, port, callback path, or select list:

```html
<div class="ocat-limited-wrapper" asp-validation-class-for="PageSize">
    <label asp-for="PageSize" class="ocat-label">@T["Page size"]</label>
    <div class="ocat-limited">
        <input asp-for="PageSize" type="number" class="form-control" />
        <span asp-validation-for="PageSize"></span>
        <span class="hint">@T["The default page size."]</span>
    </div>
</div>
```

### Limited-Width Select Row

```html
<div class="ocat-limited-wrapper" asp-validation-class-for="TimeZone">
    <label asp-for="TimeZone" class="ocat-label">@T["Default Time Zone"]</label>
    <div class="ocat-limited">
        <select asp-for="TimeZone" class="form-select">
            <option value="">@T["Local to server"]</option>
        </select>
        <span asp-validation-for="TimeZone"></span>
        <span class="hint">@T["Determines the default time zone."]</span>
    </div>
</div>
```

### Limited-Width Control Inside a Field Wrapper

Use this when the editor already needs a content field or content part wrapper on the outer row:

```html
<div class="@($"ocat-wrapper field-wrapper field-wrapper-{Model.PartFieldDefinition.PartDefinition.Name.HtmlClassify()}-{Model.PartFieldDefinition.Name.HtmlClassify()}")">
    <label asp-for="Value" class="ocat-label">@T["Value"]</label>
    <div class="ocat-end">
        <div class="ocat-limited-wrapper">
            <div class="ocat-limited">
                <input asp-for="Value" class="form-control" />
                <span asp-validation-for="Value"></span>
            </div>
        </div>
        <span class="hint">@T["Use a compact editor width while preserving the field wrapper row."]</span>
    </div>
</div>
```

### Conditional Fields with JavaScript/Liquid Syntax Selector

For fields that support both JavaScript and Liquid syntax, use the syntax selector pattern with toggling `d-none`:

```html
<div class="ocat-wrapper" asp-validation-class-for="Syntax">
    <label asp-for="Syntax" class="ocat-label">@T["Syntax"]</label>
    <div class="ocat-end">
        <select asp-for="Syntax" class="form-select">
            <option value="@nameof(WorkflowScriptSyntax.JavaScript)">@T["JavaScript"]</option>
            <option value="@nameof(WorkflowScriptSyntax.Liquid)">@T["Liquid"]</option>
        </select>
        <span asp-validation-for="Syntax"></span>
    </div>
</div>

<div id="@(Html.IdFor(m => m.Expression))_group" class="ocat-wrapper @(Model.Syntax == WorkflowScriptSyntax.JavaScript ? "" : "d-none")" asp-validation-class-for="Expression">
    <label asp-for="Expression" class="ocat-label">@T["Expression"]</label>
    <div class="ocat-end">
        <input type="text" asp-for="Expression" class="form-control code" />
        <span asp-validation-for="Expression"></span>
        <span class="hint">@T["A JavaScript expression. Example: {0}", "input(\"Count\") > 0"]</span>
    </div>
</div>

<div id="@(Html.IdFor(m => m.LiquidExpression))_group" class="ocat-wrapper @(Model.Syntax == WorkflowScriptSyntax.Liquid ? "" : "d-none")" asp-validation-class-for="LiquidExpression">
    <label asp-for="LiquidExpression" class="ocat-label">@T["Expression"]</label>
    <div class="ocat-end">
        <input type="text" asp-for="LiquidExpression" class="form-control code" />
        <span asp-validation-for="LiquidExpression"></span>
        <span class="hint">@T["A Liquid expression. Example: {0}", "{{ Workflow.Properties.Value }}"]</span>
    </div>
</div>

<script at="Foot">
    document.addEventListener('DOMContentLoaded', () => {
        const syntaxElement = document.getElementById('@Html.IdFor(m => m.Syntax)');
        const javaScriptGroup = document.getElementById('@($"{Html.IdFor(m => m.Expression)}_group")');
        const liquidGroup = document.getElementById('@($"{Html.IdFor(m => m.LiquidExpression)}_group")');

        const toggleEditors = () => {
            const useLiquid = syntaxElement.value === '@nameof(WorkflowScriptSyntax.Liquid)';
            javaScriptGroup.classList.toggle('d-none', useLiquid);
            liquidGroup.classList.toggle('d-none', !useLiquid);
        };

        syntaxElement.addEventListener('change', toggleEditors);
        toggleEditors();
    });
</script>
```

## Display-Only Fields (Read-Only)

For displaying information without a form input:

```html
<div class="ocat-wrapper">
    <label class="ocat-label">@T["Field Label"]</label>
    <div class="ocat-end">
        <span>@Model.FieldValue</span>
    </div>
</div>
```

## Migration: Old to New Classes

When updating existing views, apply these replacements:

| Old Pattern | New Pattern |
|-------------|-------------|
| `<div class="mb-3" ...>` | `<div class="ocat-wrapper" ...>` |
| `<div class="form-group" ...>` | `<div class="ocat-wrapper" ...>` |
| `<label ... class="form-label">` | `<label ... class="ocat-label">` |
| Input/select/textarea directly inside wrapper | Wrap in `<div class="ocat-end">` |
| Checkbox with label inside wrapper | Wrap in `<div class="ocat-end-offset"><div class="form-check">` |
| Standalone headline or alert directly in the form flow | Wrap in `<div class="ocat-wrapper"><div class="ocat-end-offset">...</div></div>` |
| Mixing `ocat-end` and `ocat-end-offset` in one row | Choose one wrapper based on whether there is a left-column label |
| `@Orchard.GetLimitedWidthWrapperClasses()` | `ocat-limited-wrapper` |
| `@Orchard.GetLimitedWidthClasses()` | `ocat-limited` |
| Limited-width content field row using `field-wrapper-*` | Keep outer `ocat-wrapper field-wrapper-*`, nest `ocat-limited-wrapper` inside `ocat-end` |

## Complete Edit View Example

```html
@model MyModuleViewModel

<div class="ocat-wrapper" asp-validation-class-for="Title">
    <label asp-for="Title" class="ocat-label ocat-label-required">@T["Title"]</label>
    <div class="ocat-end">
        <input asp-for="Title" class="form-control" autofocus="autofocus" />
        <span asp-validation-for="Title"></span>
        <span class="hint">@T["The title of the item."]</span>
    </div>
</div>

<div class="ocat-wrapper" asp-validation-class-for="Description">
    <label asp-for="Description" class="ocat-label">@T["Description"]</label>
    <div class="ocat-end">
        <textarea asp-for="Description" class="form-control" rows="4"></textarea>
        <span asp-validation-for="Description"></span>
        <span class="hint">@T["A brief description."]</span>
    </div>
</div>

<div class="ocat-wrapper" asp-validation-class-for="Category">
    <label asp-for="Category" class="ocat-label">@T["Category"]</label>
    <div class="ocat-end">
        <select asp-for="Category" class="form-select">
            <option value="">@T["-- Select --"]</option>
            <option value="A">@T["Category A"]</option>
            <option value="B">@T["Category B"]</option>
        </select>
        <span asp-validation-for="Category"></span>
    </div>
</div>

<div class="ocat-wrapper" asp-validation-class-for="IsPublished">
    <div class="ocat-end-offset">
        <div class="form-check">
            <input type="checkbox" class="form-check-input" asp-for="IsPublished" />
            <label asp-for="IsPublished" class="form-check-label">@T["Published"]</label>
        </div>
        <span class="hint dashed">@T["Check to publish this item."]</span>
    </div>
</div>
```

## References

- `references/ocat-classes.md` - Detailed class reference with CSS context
- Admin theme source: `src/OrchardCore.Themes/TheAdmin/`
- Example usage: `src/OrchardCore.Modules/OrchardCore.Workflows/Views/Items/`
- AGENTS.md (repo root) - Coding conventions and build commands
