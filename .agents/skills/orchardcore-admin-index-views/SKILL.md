---
name: orchardcore-admin-index-views
description: Builds OrchardCore admin index (list) pages with the AdminList shape and the admin-list tag helpers — view-declared columns, row display drivers placing shapes for both the SummaryAdmin (List) and SummaryAdminGrid (Grid) display types, column providers, row actions, toolbar, search, pager and per-list alternates. Use for every new admin page that lists items, when converting a hand-written admin listing, adding or reordering columns of an admin list, extending another module's list, or changing how its rows and actions render.
---

# OrchardCore Admin Index Views

Every admin page that lists items (an "index" page: content items, users, queries, workflows, ...) renders its
items through the shared `AdminList` shape. Never hand-write a `<ul class="list-group">`, a `<table>`, a toolbar,
a select-all checkbox, an item count, an empty state or a pager for an admin listing: the list renders all of
them, in the `List` or `Grid` layout the site and the user choose.

## Ownership

| Layer | Responsibility |
| --- | --- |
| Controller | Query the raw items and supply an `AdminListContext<TModel>` with the typed `Source`, pager, search, toolbar and other region shapes. Never build the list shape or the row shapes. |
| View | Declare the list name, the grid columns, the CSS classes, the empty message and other presentation properties with `<admin-list>`, inside the page's form. |
| Row display driver | Return ordinary `IDisplayResult` shapes, each with a `SummaryAdmin` location (List) and a `SummaryAdminGrid` location (Grid). Set optional row classes/attributes on `context.Shape`. |
| Column provider | Optional. Extend the columns a view declares, e.g. from another module. |
| `AdminList` | Resolve the layout at display time, build each row once with the display type of that layout, and render the whole listing. |

`AdminListLayout` has exactly two values: `List` renders each `SummaryAdmin` row shape whole (its row template
decides the look); `Grid` renders the `SummaryAdminGrid` row zones directly, one cell per column, each cell
rendering the zone named after its column. Layouts are not discovered or registered through templates.
Do not add column or cell drivers, column metadata classes, or build both representations of a row.

## Decide

| Situation | Do this |
| --- | --- |
| New admin page listing items | Follow the workflow below. |
| Rows already built with `BuildDisplayAsync(..., "SummaryAdmin")` and a hand-written list | Convert: move the row building out of the controller into `AdminListContext<TModel>.Source`, add the `SummaryAdminGrid` locations, then follow steps 3 to 6. |
| Rows are hard-coded `<li>` markup in Razor | Write a display driver for the row model first, so a row is a zone-holding shape, then follow the workflow. |
| Adding a column to another module's list | Step 5, plus a `SummaryAdminGrid` location in your driver. |
| The page edits a single entity | Not this skill: use `orchardcore-admin-edit-views`. |
| The page arranges items rather than listing them (a tree, a canvas, a media library, widgets by zone) | Not this skill: such pages keep their own presentation. |

## Workflow

### 1. Write the row display driver

A row is the shape `IDisplayManager<TModel>` builds for the item. Its type is `typeof(TModel).Name` plus the
display type, e.g. `IndexProfile_SummaryAdmin`, so register the driver against a domain model named exactly what
the shape should be called, never a `*ViewModel`. Register it in the `Startup` of the feature that owns the list:
a driver registered by a neighbouring feature leaves every row empty on a site where that feature is off.

Give every shape a location for each display type. The shape types and templates are the same; only the zone
differs, a row zone for `List` and the zone of a column for `Grid`:

```csharp
public override Task<IDisplayResult> DisplayAsync(IndexProfile profile, BuildDisplayContext context)
{
    // The row's classes and attributes are rendered on its row element, in both layouts.
    if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
        && context.Shape is Shape rowShape)
    {
        rowShape.Classes.Add("item");
        rowShape.Attributes["data-filter-value"] = profile.Name?.ToLowerInvariant() ?? string.Empty;
    }

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
            .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
        View("IndexProfile_ActionsMenuItems_SummaryAdmin", profile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
            .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5"));
}
```

- Every visible piece of a row (checkbox, title, badges, dates, buttons, menu items) is its own shape in a zone.
  Anything hard-coded in the row template is missing from the grid.
- A location without a display type, `.Location("Tags:5")`, applies to both display types, so in the grid the
  shape lands in a `Tags` zone no column renders. Always pair a `SummaryAdmin` location with a `SummaryAdminGrid`
  one, unless the zone name is also a column name (e.g. `Actions`).
- Display types do not inherit placement: a shape placed only for `SummaryAdmin`, by a driver or by
  `placement.json`, does not appear in the grid. A grid-only shape declares only a `SummaryAdminGrid` location.
- `placement.json` rules filtered by `"displayType": "SummaryAdminGrid"` move a shape to another column zone
  (`Source:10`), hide it (`-`), change its template or add wrappers and alternates.
- Do not use editor `|Column` groupings for cells.

### 2. Write the row template for the List layout

`{Model}.SummaryAdmin.cshtml` renders the zones of the row; the grid does not use it. Every shipped row has the
same structure, so the lists look alike:

```cshtml
<div class="row g-0 align-items-center">
    <div class="col">
        <div class="title d-flex align-items-center">
            @* Checkbox, then <div class="summary d-flex flex-column flex-md-row list-item-search-text"> with Content, Tags, Meta *@
        </div>
        @if (Model.Description != null)
        {
            @await DisplayAsync(Model.Description)
        }
    </div>
    <div class="col-auto d-flex justify-content-end ps-2">
        @await DisplayAsync(await Factory.CreateAdminListActionsAsync((IShape)Model))
    </div>
</div>
```

| Zone | Renders as | Never |
| --- | --- | --- |
| `Content` | The title, a plain link to the edit page | A heading (`<h5>`) |
| `Tags`, `Meta` | `<span class="badge ta-badge fw-normal">` per fact, leading icon, `title` tooltip | Plain text or a `<span class="hint">` |
| `Description` | `<div class="admin-list-secondary">`, its own line under the title | A second full-width row running under the actions |
| `Actions`, `ActionsMenu` | Through `AdminListActions` (`CreateAdminListActionsAsync`), so the Buttons/Menu setting applies | A hand-written button group, or `ActionsMenu` items wrapped in `<li>` |

### 3. Supply the items from the controller

```csharp
model.List = new AdminListContext<IndexProfile>
{
    Source = items,
    Pager = pagerShape,
};
```

The view model exposes `public AdminListContext List { get; set; }`. `AdminListContext<TModel>` sets `ItemType`
and exposes a typed `IEnumerable<TModel> Source`; its generic argument can select a registered base/interface
display manager, even for an empty collection of derived items. Items must be non-null instances of that type.
The list invokes the display manager after resolving its layout, so the controller has no row-rendering
dependencies and never builds rows, row callbacks or prebuilt shapes.

The default `IAdminListItemDisplayManager<TModel>` adapter calls `IDisplayManager<TModel>`. Register a closed
adapter for a specialized pipeline, e.g. content items use `IContentItemDisplayManager`.

Other context properties, all optional:

- `Header`: an options editor shape (`Summary` and `Actions` zones), as the content items and users pass.
- `Toolbar`: a toolbar shape of the page's own. Otherwise the list builds `AdminListToolbar` from the rows, the
  pager, `BulkActions`, `ShowSelectAll` and `ToolbarActions` (a shape at its end, e.g. filters).
  `ShowToolbar = false` omits it.
- `Search`, `Actions`: the search bar (e.g. the `AdminListSearch` shape) and the page buttons, in the action bar.
- `PageSize`: a separate page size selector; set `ShowPageSizeSelector = false` on the pager when passing it.
- `Data`: what a column provider may need, e.g. the content types the listing is filtered by.
- `Layout`: forces a layout, only when the page can be rendered one way, e.g. while its rows are dragged.
- `ShowLayoutSelector = false`: for a page that renders several lists or places one selector itself, by creating
  an `AdminListLayoutSelector` shape with the shared `ListName`.

### 4. Render the list in the view

Import the tag helpers once per module, in `Views/_ViewImports.cshtml`:

```cshtml
@using OrchardCore.Admin
@addTagHelper *, OrchardCore.Admin.Abstractions
```

The view keeps the form, its hidden inputs and the action bar, and renders the list with `<admin-list>`:

```cshtml
<form asp-action="Index" method="post" class="no-multisubmit bulk-select-list"
      data-checkbox-name="itemIds" data-bulk-action-name="Options.BulkAction" data-selected-text="@T["selected"]">
    <input type="submit" name="submit.Filter" id="submitFilter" class="visually-hidden" />
    <input asp-for="Options.BulkAction" type="hidden" />
    <input type="submit" name="submit.BulkAction" class="visually-hidden" />

    <admin-list name="Indexes" context="Model.List"
                bulk-actions="Model.Options.BulkActions"
                empty-message="@T["<strong>Nothing here!</strong> There are no indexes at the moment."]">
        <admin-list-list item-css-class="list-group-item" />
        <admin-list-grid>
            <admin-list-column type="Select" />
            <admin-list-column name="Name" title="@T["Name"]" position="20" />
            <admin-list-column name="Source" title="@T["Source"]" position="30" width="auto" />
            <admin-list-column type="Actions" title="@T["Actions"]" />
        </admin-list-grid>
    </admin-list>
</form>
```

- `name` and `context` are both required. The name identifies the list for column providers, the user's layout
  choice and the alternates. Use a string literal when only the view uses it; keep a module constant when it is
  reused, e.g. `ContentsAdminList.Name`, and do not create a holder class for a single use.
- The tag also takes `show-toolbar`, `show-select-all`, `show-layout-selector`, `layout`, `item-css-class`,
  `empty-message`, `bulk-actions`, and `rows-*` attributes (or `rows-attributes`) for the rows container, e.g.
  `rows-id` for a sortable list.
- `admin-list-list` and `admin-list-grid` configure their layout's `rows-class` and `item-css-class` without
  forcing it. Each can be declared once. List item classes replace the default; grid item classes are added to
  the required grid row class. Children emit no HTML.
- The hidden `submit.Filter` button stays first in the form: Enter in the search box triggers it. The search is an
  input group ending with a magnifying-glass button, never an icon inside the input.
- Use the shared `bulk-select-list` and `list-search-filter` components, wired by one small module per page
  (`Assets/ts/*.ts`), and keep the `select-all`, `items`, `selected-items` and `actions` ids the toolbar renders.

### 5. Declare the grid columns

Each `<admin-list-column>` renders the row zone named after it; `zones="A B"` renders several named zones instead.
A zone no column renders is omitted from the grid.

| Attribute | Meaning |
| --- | --- |
| `type` | `Select` (the `Select` zone, at position `10`) or `Actions` (the `Actions` and `ActionsMenu` zones, at position `end`); omit for a regular column. A preset keeps its defaults unless overridden. |
| `name` | Required for a regular column, unique ignoring case; also the default zone and the `admin-list-column-{name}` CSS class. |
| `title` | Localized header text, e.g. `@T["Name"]`; it also labels the cells when a narrow list stacks them. |
| `position` | Placement syntax (`10`, `25`, `end`); columns are sorted by it. |
| `width` | Any CSS width, or `auto` to hug the content. No width shares the remaining space. |
| `alignment`, `no-wrap`, `css-class` | Cell presentation. |

To change the columns of another module's list, implement `IAdminListColumnsProvider`:

```csharp
public sealed class CultureColumnsProvider : IAdminListColumnsProvider
{
    private readonly IStringLocalizer S;

    public CultureColumnsProvider(IStringLocalizer<CultureColumnsProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task PopulateAsync(AdminListColumnsContext context)
    {
        // Between "Title" (20) and "Type" (30), whatever order the providers run in.
        context.Columns.Add(new AdminListColumn { Name = "Culture", Position = "25", Title = S["Culture"], Width = AdminListColumn.AutoWidth });

        return Task.CompletedTask;
    }
}
```

```csharp
services.AddAdminListColumnsProvider<CultureColumnsProvider>(ContentsAdminList.Name);
```

- Registered with a list name, a provider is only created and run for that list (keyed DI with an
  `AdminListColumnsProviderKey`; names are case-sensitive). Several providers can target a list, and a provider
  can target several lists. Duplicate registrations are ignored.
- Without a name, `AddAdminListColumnsProvider<TProvider>()` registers a global provider for every list. Global
  providers run first, then the providers of the list, each group in feature dependency order. A global provider
  must tolerate different column sets: check `context.Find(name)` before changing an optional column.
- `context.Columns.Add(...)`, `context.Find(name)` and `context.Remove(name)` change the columns.
  `context.Data`, `TryGetData<T>()` and `GetData<T>()` read what the page passed, e.g.
  `ContentsAdminList.ContentTypesKey` and `ContentsAdminList.StereotypesKey`.
- A provider never builds cells: the content of its column comes from a display driver placing a shape in the
  zone of that column for `SummaryAdminGrid`.

### 6. Test and document it

- Add the page's URL to `_lists` in `test/OrchardCore.Tests.Functional/Tests/Cms/AdminListLayoutTests.cs` when the
  Blog recipe enables its feature (add an optional-list test otherwise). The test renders every list in the grid
  and fails when a column renders nothing on any row, which catches a shape missing its `SummaryAdminGrid` location.
- Check the page in both layouts and both actions layouts (Buttons and Menu), with rows and without, and in a
  narrow window, where the grid stacks its cells.
- Add the list to the table of lists in `src/docs/reference/modules/Admin/README.md`.

## Regions and properties of the shape

Rows and regions inherit the list name and layout automatically, so their alternates work without a page passing
them. `Rows` is output built during display, never an input; `DisplayType` is the resolved row display type.
Custom `AdminListCell` templates receive `Row` and `Column`. The Actions cell renders the row's `Actions` and
`ActionsMenu` zones through `AdminListActions`; moving an action to another zone removes it from that cell.

## Alternates

| Shape | Alternates, least to most specific |
| --- | --- |
| `AdminList` | `AdminList__{Layout}`, `AdminList__{Name}`, `AdminList__{Name}__{Layout}` |
| `AdminListCell` | `AdminListCell__{Column}`, `AdminListCell__{Name}__{Column}` |
| `AdminListActions` | `AdminListActions__{Layout}`, `AdminListActions__{Name}`, `AdminListActions__{Name}__{Layout}`, where `{Layout}` is `Buttons` or `Menu` |
| `AdminListToolbar`, `AdminListSearch` | `__{Layout}`, `__{Name}`, `__{Name}__{Layout}` |

`AdminListActionsLayout` has exactly two values, `Buttons` and `Menu`; configuration and the nullable site
settings use the enum. Keep templates and alternates overridable, but do not add option shapes or custom modes.

## Gotchas

1. **Never name the rows `Items`**: `Shape` already exposes `Items`, its child shapes.
2. **A location without a display type does not reach the grid column** (step 1). An empty grid column is the symptom.
3. **Per-type row templates only apply to the List layout**: `Content-BlogPost.SummaryAdmin.cshtml` does not
   affect the grid; customise it with `SummaryAdminGrid` placement or `AdminListCell-{List}-{Column}.cshtml`.
4. **Never wrap `ActionsMenu` items in `<li>`**: the dropdown sits inside the `<li>` of a List row, and the parser
   hoists a nested `<li>` out of it, so the menu renders empty and stray links appear under the list.
5. **No viewport-based classes such as `d-md-none` in list Razor**: the grid stacks with a container query on the
   list itself (`_admin-list.scss`), because the sidebar makes the viewport a poor proxy for the list width.
6. **A sortable list drags the rows, never their container**: in the grid the rows container is
   `display: contents`. Mark it with `rows-*` attributes, and send SortableJS `oldDraggableIndex`/`newDraggableIndex`,
   which count only the rows.
7. **Rows are built while the view renders**, from the `AdminList` displaying event: a driver's queries and
   errors happen during rendering, not in the controller.

## References

- `references/worked-example.md`: the Indexes page, driver, controller and view.
- `src/docs/reference/modules/Admin/README.md`: the user and developer documentation of admin lists.
- `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminList*.cshtml`: the layout, cell, actions and toolbar templates.
- `src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_admin-list.scss`: the layout CSS and its container queries.
- `test/OrchardCore.Tests.Functional/Tests/Cms/AdminListLayoutTests.cs`: renders every list in the grid.
