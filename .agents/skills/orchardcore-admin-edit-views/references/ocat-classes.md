# OrchardCore Admin Edit Views - `ocat-*` CSS Classes Reference

The `ocat-*` CSS class system was introduced in OrchardCore to provide a consistent two-column layout for admin form fields. These classes are used in all admin edit views throughout the repository.

## Class Reference

### `ocat-wrapper`

**Replaces:** `mb-3`, `form-group`  
**Element:** `<div>`  
**Purpose:** Outer container for a single form field row. Creates the two-column layout.  
**Always includes:** `asp-validation-class-for="FieldName"` (for fields with validation)

```html
<div class="ocat-wrapper" asp-validation-class-for="FieldName">
    <!-- label and ocat-end go here -->
</div>
```

### `ocat-label`

**Replaces:** `form-label`  
**Element:** `<label>`  
**Purpose:** Styles the label in the left column of the field row.

```html
<label asp-for="FieldName" class="ocat-label">@T["Label Text"]</label>
```

### `ocat-label-required`

**Used with:** `ocat-label`  
**Element:** `<label>`  
**Purpose:** Marks a field as required with a visual indicator. Always combine with `ocat-label`.

```html
<label asp-for="FieldName" class="ocat-label ocat-label-required">@T["Required Field"]</label>
```

### `ocat-end`

**Element:** `<div>`  
**Purpose:** Wraps the input control, validation message, and hint text in the right column.  
**Use when:** The field has a label in the left column.

```html
<div class="ocat-end">
    <input asp-for="FieldName" class="form-control" />
    <span asp-validation-for="FieldName"></span>
    <span class="hint">@T["Hint text."]</span>
</div>
```

### `ocat-end-offset`

**Element:** `<div>`  
**Purpose:** Right-column content with no left-column label. Visually aligns the content with `ocat-end` elements.  
**Use when:** The field has no separate label (e.g., checkboxes where the label is inline with the input), or when a standalone alert, legend, or section heading belongs in the form flow.

```html
<div class="ocat-end-offset">
    <div class="form-check">
        <input type="checkbox" class="form-check-input" asp-for="IsEnabled" />
        <label asp-for="IsEnabled" class="form-check-label">@T["Enable"]</label>
    </div>
    <span class="hint dashed">@T["Hint text."]</span>
</div>
```

## Rules

1. **Every field row** must use `ocat-wrapper` as the outer container.
2. **Fields with a label** must use `ocat-label` on the label element and `ocat-end` around the input.
3. **Checkboxes/toggles** use `ocat-end-offset` (no `ocat-label`) to keep alignment.
4. **Validation spans** (`<span asp-validation-for="...">`) always go inside `ocat-end` or `ocat-end-offset`.
5. **Hint spans** (`<span class="hint">`) always go inside `ocat-end` or `ocat-end-offset`.
6. **Standalone alerts, legends, and section headings** in edit forms should use their own `ocat-wrapper` + `ocat-end-offset` row.
7. **Never mix** `ocat-end` and `ocat-end-offset` in the same row; choose one based on whether the row has a left-column label.
8. **Dynamic visibility** (show/hide based on selection): add `d-none` to `ocat-wrapper` directly, not to an inner wrapper:

```html
<div id="myField_group" class="ocat-wrapper @(Model.IsAdvanced ? "" : "d-none")" asp-validation-class-for="MyField">
    <label asp-for="MyField" class="ocat-label">@T["Label"]</label>
    <div class="ocat-end">
        <input asp-for="MyField" class="form-control" />
        <span asp-validation-for="MyField"></span>
    </div>
</div>
```

## Common Mistakes to Avoid

| ❌ Wrong | ✅ Correct |
|---------|-----------|
| `<div class="mb-3">` | `<div class="ocat-wrapper">` |
| `<label class="form-label">` | `<label class="ocat-label">` |
| Input directly inside `ocat-wrapper` | Input inside `<div class="ocat-end">` |
| `<span class="hint">` directly in `ocat-wrapper` | `<span class="hint">` inside `ocat-end` |
| Checkbox inside `ocat-end` (with label in left column) | Checkbox inside `ocat-end-offset` (no left label) |
| Standalone alert or headline directly in the form flow | Standalone alert or headline inside `ocat-wrapper` + `ocat-end-offset` |
| Row containing both `ocat-end` and `ocat-end-offset` | Row containing exactly one of the two wrappers |

## File Locations

Edit views for admin features are located in:
- Content parts: `src/OrchardCore.Modules/OrchardCore.{Module}/Views/{PartName}.Edit.cshtml`
- Settings: `src/OrchardCore.Modules/OrchardCore.{Module}/Views/{Settings}.Edit.cshtml`
- Workflow tasks: `src/OrchardCore.Modules/OrchardCore.Workflows/Views/Items/{TaskName}.Fields.Edit.cshtml`
- Site settings: `src/OrchardCore.Modules/OrchardCore.{Module}/Views/{Feature}Settings.Edit.cshtml`
