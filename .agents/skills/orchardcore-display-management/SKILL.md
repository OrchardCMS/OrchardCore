---
name: orchardcore-display-management
description: Controls how OrchardCore renders content — placement.json rules, display drivers, shapes, zones, alternates, and editor groupings (tabs/cards/columns). Use when the user needs to position or hide a part/field, move a shape to another zone, add alternates or wrappers, build admin editor layouts, or write a display driver.
---

# OrchardCore Display Management

This skill guides you through OrchardCore's display system following project conventions.

OrchardCore renders content by building **shapes** (dynamic view models) from drivers, then placing each shape into a **zone** of the layout. **Drivers** decide *what* shapes exist and their default location; **`placement.json`** overrides *where* they go, *whether* they show, and *which* template renders them.

## Mental model

```
ContentItem ──drivers──▶ shapes ──placement.json──▶ zones ──templates──▶ HTML
```

- **Shape** — a dynamic object with a `ShapeType` and `Metadata`. The shape type (e.g. `TextField`, `Parts_Contents_Publish`, `ContentPart`) selects the template and the placement rule.
- **Driver** — `DisplayDriver` / `ContentPartDisplayDriver` / `ContentFieldDisplayDriver`. Returns `IDisplayResult`s from `Display`, `Edit`, `UpdateAsync`, each with a default `.Location(...)`.
- **Placement** — `placement.json` at the **root** of a module or theme. Filters shapes and rewrites their location/visibility/template.
- **Zone** — named region of the layout (`Content`, `Header`, `Footer`, `Actions`, …) or an editor group (tab/card/column).

## Decide: driver vs placement

| Want to… | Use |
|----------|-----|
| Reposition / hide a shape someone else built | `placement.json` |
| Move a shape to a layout zone | `placement.json` (`place` starts with `/`) | 
| Swap the template or add alternates | `placement.json` (`shape` / `alternates`) |
| Lay out the admin editor (tabs/cards/columns) | `placement.json` groupings |
| Produce a *new* shape from your part/field | display driver |
| Read/write the editor model | display driver (`Edit` / `UpdateAsync`) |

Prefer `placement.json` for layout — no rebuild needed, theme can override.

## Workflow A: place a shape

### Step 1: Find the shape type and differentiator

The placement property name is the **shape type**, not the part name. A part **without** its own driver renders as `ContentPart` (differentiator = part name). A field renders as its field type (`TextField`) with differentiator `{PartName}-{FieldName}`.

See `references/placement.md` for the full differentiator table.

### Step 2: Add a rule to `placement.json`

```json
{
  "TextField": [
    {
      "differentiator": "Article-Subtitle",
      "displayType": "Detail",
      "place": "Content:2"
    }
  ]
}
```

### Step 3: Verify

Run the site, view the content item. Use `?display` debug shapes or the template's shape tracing to confirm the shape type. Wrong type → no rule matches.

## Workflow B: write a display driver

### Step 1: Inherit the right base

| Base class | For |
|------------|-----|
| `ContentPartDisplayDriver<TPart>` | a content part |
| `ContentFieldDisplayDriver<TField>` | a content field |
| `SectionDisplayDriver<TModel, TSection>` | settings sections |
| `DisplayDriver<TModel>` | anything else |

### Step 2: Implement Display / Edit / Update

```csharp
public sealed class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
{
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

    public override IDisplayResult Edit(TextField field, BuildFieldEditorContext context)
    {
        return Initialize<EditTextFieldViewModel>(GetEditorShapeType(context), model =>
        {
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(TextField field, UpdateFieldEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(field, Prefix, f => f.Text);
        return Edit(field, context);
    }
}
```

### Step 3: Register the driver

In `Startup.cs`:

```csharp
services.AddContentPartDisplayDriver<MyPart, MyPartDisplayDriver>();
// or for a field:
services.AddContentFieldDisplayDriver<TextField, TextFieldDisplayDriver>();
```

See `references/drivers.md` for shape-type helpers, the fluent `.Location` API, and registration variants.

## Quick Reference

### Placement rule keys

| Key | Purpose |
|-----|---------|
| `place` | `Zone:Position`. `-` hides, `/Zone` targets a layout zone |
| `displayType` | Match `Detail`, `Summary`, `SummaryAdmin`, `DetailAdmin`, `Edit` |
| `differentiator` | Distinguish reused shapes (field/part name) |
| `contentType` | Match content type / stereotype (`Art*` wildcard, or array) |
| `contentPart` | Match content items containing a part |
| `path` | Match request path |
| `alternates` | Add alternate shape types |
| `wrappers` | Wrap the shape in other shapes |
| `shape` | Substitute a different shape type |

### Position values

`before` < `0`/empty < `1` < `1.1` < `2` < `10` < `after`. Numeric, not alphabetic. Dot notation inserts between (`5.1` is between 5 and 6).

### Editor grouping modifiers

| Modifier | Group | Example |
|----------|-------|---------|
| `#` | Tab | `Parts:0#Media;1` |
| `%` | Card | `Parts:0%Details;2` |
| `\|` | Column | `Parts:0\|Left_9;1` (`_9` = width, `;1` = order) |

`;N` after a group name = group order. `_N` after a column name = Bootstrap grid width (12-col).

### Common zones

| Zone | Purpose |
|------|---------|
| `Content` | Main content shapes |
| `Header` / `Footer` | Layout regions |
| `Actions` | Admin list row buttons |
| `Parts` | Editor parts container |
| `Meta` | Metadata block |

## Gotchas

- Hiding a whole editor row → target `ContentPart_Edit` with differentiator `{ContentType}-{PartName}`, not the inner `XxxPart_Edit`.
- Shape type ≠ part type. No driver → shape is `ContentPart`. Summary display of a dynamic part → `ContentPart_Summary`.
- Field display modes are stricter: shape type needs `_Display`, differentiator is `{Part}-{Field}-{FieldType}_Display__{Mode}`.
- Placement precedence: startup project → active theme → modules (by dependency). A theme can override any module's placement.
- Same position value → registration order decides; use distinct positions when order matters.

## References

- `references/placement.md` — placement.json filters, differentiators, layout zones, editor groupings
- `references/drivers.md` — driver base classes, shape helpers, fluent Location API, registration
- `src/docs/reference/modules/Placement/README.md` (repo) — official placement reference
- `src/docs/reference/modules/DisplayManagement/README.md` (repo) — display management reference
- `src/docs/reference/modules/Templates/README.md` (repo) — shape differentiators and alternates
- `AGENTS.md` (repo root) — build commands
