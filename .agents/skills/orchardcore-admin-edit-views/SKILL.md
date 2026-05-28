---
name: orchardcore-admin-edit-views
description: Creates and updates OrchardCore admin edit views using the ocat-* CSS class conventions. Use when creating or modifying Edit view files (*.Edit.cshtml, *.Fields.Edit.cshtml) to ensure correct layout and styling.
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
