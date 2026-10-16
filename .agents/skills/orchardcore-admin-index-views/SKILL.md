---
name: orchardcore-admin-index-views
description: Builds OrchardCore admin index pages with the AdminList shape, view-defined columns, column providers, and SummaryAdmin/SummaryAdminGrid row display types. Use when creating or converting admin listings, extending columns, or changing rows and actions.
---

# OrchardCore Admin Index Views

## Ownership

| Layer | Responsibility |
| --- | --- |
| Controller | Query raw items and supply an `AdminListContext<TModel>` with typed `Source`, pager, search, toolbar and other shapes. Do not build list or row shapes. |
| View | Declare the list name, columns, CSS, empty message and other presentation properties; render the `AdminList` shape inside the form. |
| Row display drivers | Return standard `IDisplayResult` shapes with separate `SummaryAdmin` and `SummaryAdminGrid` zone locations; set optional row attributes on `context.Shape`. |
| Column providers | Extend the view-defined columns, including when the list has no rows. |
| `AdminList` | Resolve layout at display time, build only the selected display type and render the whole listing. |

`AdminListLayout` has exactly two values: `List` renders `SummaryAdmin` rows whole; `Grid` renders
`SummaryAdminGrid` row zones directly. Layouts are not discovered or registered through templates.
Do not add separate column/cell drivers, column metadata, or build both representations of a row.

## 1. Build row shapes

Reuse the same shape type and template with a location for each display type:

```csharp
public override Task<IDisplayResult> DisplayAsync(IndexProfile profile, BuildDisplayContext context)
{
    return CombineAsync(
        View("IndexProfile_Checkbox_SummaryAdmin", profile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
            .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
        View("IndexProfile_Fields_SummaryAdmin", profile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
            .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
        View("IndexProfile_DefaultTags_SummaryAdmin", profile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
            .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Source:5"),
        View("IndexProfile_Buttons_SummaryAdmin", profile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
            .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"));
}
```

Normal `placement.json` rules filtered by `displayType: "SummaryAdminGrid"` move a shape to another column zone
(`Source:10`), hide it (`-`), change its template or add wrappers/alternates. Display types do not inherit placements.
Declare only a `SummaryAdminGrid` location for a grid-only shape. Do not use editor `|Column` groupings for cells.

## 2. Defer rows until the display type is known

```csharp
model.List = new AdminListContext<IndexProfile>
{
    Source = items,
    Pager = pagerShape,
};
```

`AdminListContext<TModel>` derives from `AdminListContext`, sets `ItemType` automatically and exposes
`IEnumerable<TModel> Source`. Its generic argument can select a registered base/interface display manager,
even with an empty collection of derived items. The tag accepts either context version.
The tag creates the shape, and the list builds rows once after columns and layout are resolved.
The non-generic context can infer the collection's unambiguous
`IEnumerable<T>` type or use an explicit `ItemType`. Items must be non-null and assignable
to the selected type.

The default `IAdminListItemDisplayManager<TModel>` adapter calls `IDisplayManager<TModel>`.
Register a closed adapter for specialized pipelines, e.g. `ContentItem` uses `IContentItemDisplayManager`.
Set model-specific row classes/attributes in the display driver for both display types, e.g.
`context.Shape`'s `Classes.Add("item")` and `Attributes["data-filter-value"]` for client-side search.
Do not introduce per-row controller callbacks.

## 3. Declare columns in the rendering view

```cshtml
@addTagHelper *, OrchardCore.Admin.Abstractions
<admin-list name="Indexes" context="Model.List" bulk-actions="Model.Options.BulkActions" empty-message="@T["No results found."]">
    <admin-list-list item-css-class="list-group-item" />
    <admin-list-grid>
        <admin-list-column type="Select" />
        <admin-list-column name="Name" title="@T["Name"]" position="20" />
        <admin-list-column name="Source" title="@T["Source"]" position="30" width="auto" />
        <admin-list-column type="Actions" title="@T["Actions"]" />
    </admin-list-grid>
</admin-list>
```

The parent creates the named shape from the context and renders it after evaluating child declarations.
The context does not carry a name. Specify both `name` and `context`; the name identifies this
rendering for column providers, layout preferences and alternates.
The tag does not accept direct items or an existing shape.
`rows-id` and other `rows-*` attributes, or `rows-attributes`, configure the rows container in the view.
Children emit no HTML.
Columns may also be direct `admin-list` children. `admin-list-list` and `admin-list-grid` configure their own
`rows-class` and `item-css-class` without forcing a layout. List item classes replace the default; Grid item
classes are added to the required grid row class. Rows classes merge with `RowsAttributes["class"]`.
Each layout can be declared once. Column `type` presets preserve defaults unless explicitly overridden.

Initialization runs during display, before alternates are chosen. Use the context and tag as the public
list-building API; shape creation and row building are internal implementation details.

Column names are unique ignoring case. `Position` sorts them; `end` keeps actions last.
`Width = AdminListColumn.AutoWidth` hugs content, no width shares remaining space.
`Alignment`, `NoWrap` and `CssClass` customize cells. A cell renders the zone named after the column;
optional `Zones` renders several explicitly named row zones instead. Zones without a column are omitted.

## 4. Extend columns

Implement `IAdminListColumnsProvider.PopulateAsync(AdminListColumnsContext context)`.
Use `context.Columns.Add(...)`, `context.Find(name)` or `context.Remove(name)`.
Register with `services.AddAdminListColumnsProvider<TProvider>(listName)`; duplicate registrations for
the same provider and list are ignored. Multiple providers can target a list; a provider can target several lists.
Only matching named providers are constructed and run. Resolution uses keyed DI with an
`AdminListColumnsProviderKey` record key, not a plain string. List names are case-sensitive.
Omit the name, `services.AddAdminListColumnsProvider<TProvider>()`, for an unkeyed scoped global provider
that runs for every list building columns. Global providers must tolerate different column sets; check
`context.Find(name)` before modifying an optional column. Duplicate global registrations are ignored.
Global providers run before matching named providers, in registration/feature dependency order within
each group, after the view declarations and before columns are sorted. Registering the same provider
globally and for a list invokes it twice for that list.
Providers do not build cells; use the normal row display driver to supply shapes.

Use a string literal for a list name used only once in its module. Keep a module constant when its name
is reused, or a shared constant in an abstractions project; do not create a holder class just for one use.

The page can supply contextual `AdminListContext.Data`. Providers read `context.Data`,
`TryGetData<T>()` or `GetData<T>()`. Contents supplies `ContentsAdminList.ContentTypesKey`
and `ContentsAdminList.StereotypesKey` when the route is filtered.

## Shape regions

- `Header`: options editor shape (`Summary` and `Actions` zones).
- `Toolbar`: a custom toolbar shape. Otherwise one is built from rows, pager, `BulkActions`, `ShowSelectAll` and `ToolbarActions`; `ShowToolbar = false` omits it.
- `Search`, `Actions`: search bar and page buttons; only an action bar carrying search is sticky.
- `Pager`, `PageSize`: footer shapes. Set `ShowPageSizeSelector = false` on a pager when supplying a separate selector.
- `RowsAttributes`: attributes of the rows container, e.g. a sortable-list ID.
- `ListOptions`, `GridOptions`: `AdminListLayoutOptions` holding `RowsCssClass` and `ItemCssClass` for one layout; the layout tags set these properties.
- `Layout`: optional `AdminListLayout` value, normally resolved from site/user settings; force a layout only when needed.
- `ShowLayoutSelector`: hides the List/Grid switcher when false. For a shared page-level selector, create
  `AdminListLayoutSelector` with `ListName`; it resolves its current layout and URLs when displayed.
- `Source`, `ItemType`: raw models and optional display model type. `Items` is reserved by the shape.
- `Rows`: output built during display, never an input. Supply `Source` models, not row callbacks or prebuilt shapes.
- `DisplayType`: resolved row display type. Custom `AdminListCell` templates receive `Row` and `Column`.
- `Headers`: optional zone-holding shape with a zone per column, otherwise the title is used.

The Actions cell renders the row's `Actions` and `ActionsMenu` zones through `AdminListActions`.
Moving an action to another zone removes it from the actions cell naturally.

## Forms and row conventions

Keep hidden `submit.Filter` first, hidden bulk-action inputs and antiforgery handling.
Use the shared `bulk-select-list` and `list-search-filter` components; preserve IDs `select-all`, `items`, `selected-items` and `actions`.
Search is an input group with a trailing magnifying-glass button.

Rows use `row g-0 align-items-center`: a `col` holds the title, tags/meta and description; a `col-auto`
holds `@await DisplayAsync(await Factory.CreateAdminListActionsAsync((IShape)Model))`.
Titles are plain links, tags/meta are `badge ta-badge fw-normal`, descriptions use `admin-list-secondary`.
Menu items are bare anchors/buttons, never nested `li` elements.

Never name rows `Items` (reserved by `Shape`). Never use viewport-based responsive logic in layout Razor:
the theme uses container queries. SortableJS must use `oldDraggableIndex`/`newDraggableIndex` and drag rows,
not the `display: contents` container of the grid layout.

## Alternates

`AdminList`: `__{Layout}`, `__{Name}`, `__{Name}__{Layout}`.
`AdminListCell`: `__{Column}`, `__{Name}__{Column}`.
`AdminListActions`, `AdminListToolbar`, `AdminListSearch`: layout, list and list/layout variants.
Actions layout means Buttons/Menu; other layout variants mean List/Grid.
`AdminListActionsLayout` has exactly two values, Buttons and Menu. Configuration and nullable site settings
use the enum. Keep the templates and alternates overridable, but do not add option shapes or custom modes.
Rows and regions inherit the list name/layout automatically.

## References

- `references/worked-example.md`
- `src/docs/reference/modules/Admin/README.md`
- `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminList*.cshtml`
- `src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_admin-list.scss`
