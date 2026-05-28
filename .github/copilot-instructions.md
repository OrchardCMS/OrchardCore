# Copilot Instructions

## Admin edit view conventions

When creating or updating Orchard Core admin edit views, always use the `ocat-*` admin theme classes so the UI stays consistent across the site and works with custom admin theme overrides.

This applies to admin-facing Razor edit views such as:

- `*.Edit.cshtml`
- `*.Fields.Edit.cshtml`
- Admin editor templates rendered in the admin theme

### Required patterns

| Scenario | Required structure |
|---|---|
| Standard admin field row | `ocat-wrapper` + `ocat-label` + `ocat-end` |
| Required field label | `ocat-label ocat-label-required` |
| Checkbox or toggle with no left-column label | `ocat-wrapper` + `ocat-end-offset` |
| Limited-width row for short text, numbers, selects, paths, IDs, ports, or compact settings inputs | `ocat-limited-wrapper` + `ocat-label` + `ocat-limited` |

### Standard field example

```cshtml
<div class="ocat-wrapper" asp-validation-class-for="DisplayText">
    <label asp-for="DisplayText" class="ocat-label">@T["Display text"]</label>
    <div class="ocat-end">
        <input asp-for="DisplayText" class="form-control" />
        <span asp-validation-for="DisplayText"></span>
        <span class="hint">@T["Shown to editors in the admin UI."]</span>
    </div>
</div>
```

### Limited-width field example

```cshtml
<div class="ocat-limited-wrapper" asp-validation-class-for="PageSize">
    <label asp-for="PageSize" class="ocat-label">@T["Page size"]</label>
    <div class="ocat-limited">
        <input asp-for="PageSize" type="number" class="form-control" />
        <span asp-validation-for="PageSize"></span>
        <span class="hint">@T["The default page size."]</span>
    </div>
</div>
```

### Limited-width field inside a content field or content part wrapper

If the row also needs Orchard-specific wrapper classes such as `field-wrapper-*` or `content-part-wrapper-*`, keep the outer `ocat-wrapper` and place the compact control inside `ocat-end`:

```cshtml
<div class="ocat-wrapper field-wrapper @($"field-wrapper-{Model.PartFieldDefinition.PartDefinition.Name.HtmlClassify()}-{Model.PartFieldDefinition.Name.HtmlClassify()}")">
    <label asp-for="Value" class="ocat-label">@T["Value"]</label>
    <div class="ocat-end">
        <div class="ocat-limited-wrapper">
            <div class="ocat-limited">
                <input asp-for="Value" class="form-control" />
                <span asp-validation-for="Value"></span>
            </div>
        </div>
        <span class="hint">@T["Keeps a compact editor width without losing the field wrapper row."]</span>
    </div>
</div>
```

### Avoid

- `mb-3`, `form-group`, or `form-label` as the row layout pattern
- Legacy helper methods such as `@Orchard.GetWrapperClasses()` or `@Orchard.GetLimitedWidthWrapperClasses()`
- Full-width `ocat-wrapper` rows for inputs that should clearly remain compact unless the existing view already follows that pattern intentionally
