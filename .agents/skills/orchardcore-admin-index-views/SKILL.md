---
name: orchardcore-admin-index-views
description: Builds OrchardCore admin index (list) pages with the AdminList shape — switchable List/Table/Grid layouts, columns mapped to row zones, IAdminListService, IAdminListColumnProvider, toolbars, row actions, and client-side search. Use when creating or converting an admin listing page (Index.cshtml, *AdminList.cshtml), adding or reordering its columns, or changing how its rows and actions render.
---

# OrchardCore Admin Index Views

This skill guides you through building an admin index page (a listing screen such as **Manage Content**, **Users**, or **Indexes**) on top of the reusable `AdminList` shape, so the page inherits the site-wide layout and row-action settings instead of hard-coding its own markup.

## Mental model

An admin index page has three layers. Only the first is written per page.

| Layer | Owner | What it does |
|-------|-------|--------------|
| Action bar | The page's own view | The sticky card above the list: search box and the create button. |
| `AdminList` shape | `OrchardCore.Admin` | Renders the toolbar, the rows and the pager in the configured **layout**. |
| Row shapes | Display drivers | One `SummaryAdmin` shape per item, its zones filled by drivers and `placement.json`. |

The layout only decides **how** the already-built rows are presented:

The layout comes from `AdminListOptions`, which binds the `OrchardCore:AdminList` configuration section and is then overridden by the site settings. It follows the signal-backed options pattern, so consumers read `IOptionsMonitor<AdminListOptions>.CurrentValue` and the settings driver calls `IOptionsUpdateNotifier.RequestUpdate<AdminListOptions>()`. Never hard-code a layout name in a page; resolve it with `IAdminListService`.

- `List` renders each row shape whole, so the row template (`Content.SummaryAdmin.cshtml`) decides the look.
- `Table` and `Grid` render one **column** per `AdminListColumn`, and a column is a set of **row zones**. The row template is not used.

Because a column is just a list of zones, adding a column never means touching the layout templates — it means placing a shape in a zone.

## Decide: does this page qualify?

| Situation | Do this |
|-----------|---------|
| Rows are already built with `BuildDisplayAsync(..., "SummaryAdmin")` | Convert directly, follow the workflow below. |
| Rows are hard-coded `<li>` markup in Razor | Write a display driver for the model first so rows become zone-holding shapes, then convert. |
| The page shows one entity, not a list | Not this skill. Use `orchardcore-admin-edit-views`. |

## Workflow

### Step 1: Make sure the row shape has the zones you need

Each column reads zones from the row shape. Place a shape into a zone from a display driver:

```csharp
public override Task<IDisplayResult> DisplayAsync(IndexProfile indexProfile, BuildDisplayContext context)
{
    return CombineAsync(
        View("IndexProfile_Checkbox_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
        View("IndexProfile_Fields_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
        View("IndexProfile_DefaultTags_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
        View("IndexProfile_Buttons_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),
        View("IndexProfile_ActionsMenuItems_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
    );
}
```

Anything still hard-coded in the row template (a title link, a type badge, a checkbox) must move into its own shape and zone, or it will vanish in the column layouts.

### Step 2: Declare the list name and its default columns

One static class per list, so the controller, the tests and any column provider share it:

```csharp
public static class IndexingAdminList
{
    public const string Name = "Indexes";

    public static List<AdminListColumn> GetDefaultColumns(IStringLocalizer S) =>
    [
        new()
        {
            Name = "Select",
            Position = "10",
            Zones = ["Checkbox"],
            CssClass = "admin-list-select",
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            // No width: this column takes the space left by the others.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content"],
        },
        new()
        {
            Name = "Source",
            Position = "30",
            Title = S["Source"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
            // "end" keeps the actions last even when a feature appends a column.
            Name = "Actions",
            Position = "end",
            Title = S["Actions"],
            Zones = ["Actions", "ActionsMenu"],
            Width = AdminListColumn.AutoWidth,
            Alignment = AdminListColumnAlignment.End,
        },
    ];
}
```

### Step 3: Build the `AdminList` shape in the controller

Inject `IAdminListService` per action with `[FromServices]` and pass the request token:

```csharp
public async Task<IActionResult> Index(
    [FromServices] IShapeFactory shapeFactory,
    [FromServices] IAdminListService adminListService,
    PagerParameters pagerParameters)
{
    // ... query, pager and row shapes ...

    viewModel.List = await shapeFactory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
    {
        Name = IndexingAdminList.Name,
        Layout = await adminListService.GetLayoutAsync(IndexingAdminList.Name, cancellationToken: HttpContext.RequestAborted),
        Columns = await adminListService.GetColumnsAsync(IndexingAdminList.Name, IndexingAdminList.GetDefaultColumns(S), HttpContext.RequestAborted),
        Rows = rows,
        Toolbar = toolbar,
        Pager = viewModel.Pager,
        EmptyMessage = H["There are no indexes at the moment."],
    }));

    return View(viewModel);
}
```

### Step 4: Render it

The page keeps its own action bar and delegates the rest:

```html
<div class="card text-bg-theme mb-3 position-sticky action-bar">
    <div class="card-body">
        <div class="row gx-2">
            <div class="col">
                <div class="input-group has-search">
                    <i class="fa-solid fa-search form-control-feedback" aria-hidden="true"></i>
                    <input id="search-box" asp-for="Options.Search" class="form-control" placeholder="@T["Search"]" type="search" autocomplete="off" />
                    <button type="submit" name="submit.Filter" class="btn btn-outline-secondary" title="@T["Search"]">@T["Go"]</button>
                </div>
            </div>
            <div class="col-auto">
                @* create button *@
            </div>
        </div>
    </div>
</div>

@* Rows, toolbar and pager, in the configured layout. *@
@await DisplayAsync(Model.List)
```

### Step 5: Wire the toolbar

Two ways to fill the strip above the rows:

- **`Header`** — the options editor shape from `IDisplayManager<TOptions>.BuildEditorAsync()`. Its `Summary` zone renders on the left (count, select-all) and its `Actions` zone on the right (filters, bulk actions). Use this when the page already has an options editor, as Contents and Users do.
- **`Toolbar`** — any shape, rendered as is. The generic `AdminListToolbar` shape covers the common case:

```csharp
var toolbar = await shapeFactory.CreateAsync("AdminListToolbar", Arguments.From(new
{
    ItemsCount = viewModel.Models.Count,
    TotalItemCount = result.Count,
    StartIndex = viewModel.Models.Count > 0 ? pager.GetStartIndex() + 1 : 0,
    EndIndex = pager.GetStartIndex() + viewModel.Models.Count,
    BulkActions = viewModel.Options.BulkActions,
}));
```

## Quick reference

### `AdminList` shape properties

| Property | Description |
|----------|-------------|
| `Name` | List name, e.g. `Contents`. Drives the `__{Name}` alternates and the column providers. |
| `Layout` | From `IAdminListService.GetLayoutAsync()`. |
| `Columns` | From `IAdminListService.GetColumnsAsync()`. |
| `Rows` | The row shapes. **Never call this `Items`** — see Gotchas. |
| `Header` | Options editor shape (`Summary` + `Actions` zones). |
| `Toolbar` | Alternative to `Header`; a shape rendered as is. |
| `Search` | Optional. The search bar, usually the `AdminListSearch` shape (`Name`, `Value`, `Placeholder`, `Id`, `SubmitName`, `Autofocus`, and a `Filters` zone before the input). |
| `Actions` | Optional. The buttons of the page, e.g. "Add", rendered beside the search. |
| `PageSize` | Optional. The page size selector, built with `PageSizeSelector.BuildOptions()`. Set `ShowPageSizeSelector = false` on the pager when you pass it. |
| `Pager` | The pager shape. It already contains the page size selector. |
| `ItemCssClass` | Per-item classes in the `List` layout. |
| `EmptyMessage` | Message when there are no rows. |

### `AdminListColumn` properties

| Property | Description |
|----------|-------------|
| `Name` | Technical name. Drives `AdminListCell__{Name}` alternates and the `admin-list-column-{name}` class. |
| `Title` | Localized header. `null` for a column with no header, e.g. the checkbox. |
| `Zones` | Row zones rendered in the cell, in order. |
| `Position` | Placement syntax (`10`, `25`, `end`). Sorted after every provider ran. |
| `Width` | Any CSS width, or `AdminListColumn.AutoWidth` to hug the content. No width means "share the rest". |
| `Alignment` | `Start`, `Center`, `End`. |
| `NoWrap` | Keeps the cell on one line. |
| `CssClass` | Extra classes on the header and the cells. |

### Alternates

| Shape | Alternates (least to most specific) |
|-------|--------------------------------------|
| `AdminList` | `AdminList__{Layout}`, `AdminList__{Name}`, `AdminList__{Name}__{Layout}` |
| `AdminListCell` | `AdminListCell__{Column}`, `AdminListCell__{ListName}__{Column}` |
| `AdminListActions` | `AdminListActions__{Layout}`, `AdminListActions__{ListName}`, `AdminListActions__{ListName}__{Layout}` |

### Adding a layout — two files, no C#

`AdminList-Cards.cshtml` renders the list, and `AdminListLayout-Cards.Option.cshtml` makes it selectable:

```html
@{
    string current = Model.ListLayout;
}
<option value="Cards" selected="@(current == "Cards")">@T["Cards"]</option>
```

The settings page discovers layouts by scanning the shape table for `AdminListLayout_Option__*`, exactly as the content type editor discovers field editors from `{Field}_Option__*`. Row-action layouts work the same way with `AdminListActions_Option__*`.

### Adding a column from another feature

```csharp
public sealed class CultureColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public CultureColumnProvider(IStringLocalizer<CultureColumnProvider> stringLocalizer)
        => S = stringLocalizer;

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        if (context.ListName == "Contents")
        {
            // Between "Title" (20) and "Type" (30), whatever order the providers run in.
            context.Columns.Add(new AdminListColumn
            {
                Name = "Culture",
                Position = "25",
                Title = S["Culture"],
                Zones = ["Culture"],
            });
        }

        return Task.CompletedTask;
    }
}
```

```csharp
services.AddAdminListColumnProvider<CultureColumnProvider>();
```

Every part of a list carries the name of the list, so each part can be overridden for one list alone:
`AdminList-{List}.cshtml`, `AdminList-{List}-{Layout}.cshtml`, `AdminListCell-{List}-{Column}.cshtml`,
`AdminListActions-{List}.cshtml`, `AdminListActions-{List}-{ActionsLayout}.cshtml`,
`AdminListToolbar-{List}.cshtml`, `AdminListToolbar-{List}-{Layout}.cshtml`, `AdminListSearch-{List}.cshtml`
and `AdminListSearch-{List}-{Layout}.cshtml`. The `AdminList` shape stamps its name and its layout on the
toolbar, the search bar, the pager, the page size selector and every row, so a row template renders
`AdminListActions` without passing a list name and still gets those alternates. `{Layout}` is the layout of
the list, except on `AdminListActions`, where it is `Buttons` or `Menu`.

`context.Find(name)` and `context.Remove(name)` alter existing columns. Never rely on the position of a column in the collection; use `Position`.

A provider often needs to know more than the name of the list: the content items list is the same list whether it shows every item or only the blog posts. The page passes what it knows as the `data` argument of `GetColumnsAsync`, and the provider reads it back from the context:

```csharp
var data = new Dictionary<string, object>
{
    [ContentsAdminList.ContentTypesKey] = new[] { "BlogPost" },
};

var columns = await adminListService.GetColumnsAsync(ContentsAdminList.Name, ContentsAdminList.GetDefaultColumns(S), data, HttpContext.RequestAborted);
```

```csharp
if (context.ListName == ContentsAdminList.Name &&
    context.TryGetData<string[]>(ContentsAdminList.ContentTypesKey, out var contentTypes) &&
    contentTypes.Contains("BlogPost"))
{
    context.Columns.Add(new AdminListColumn { Name = "Category", Position = "25", Title = S["Category"], Zones = ["Category"] });
}
```

`context.Data` is never null, and `TryGetData<T>` / `GetData<T>` fall back instead of throwing on a missing key or another type. A list documents the keys it fills: the content items list fills `ContentsAdminList.ContentTypesKey` and `ContentsAdminList.StereotypesKey` from its route, and leaves them out when the listing is not filtered.

## Gotchas

1. **Do not name the rows property `Items`.** `Shape` already exposes `Items` (its child shapes), so `Model.Items` in the template silently resolves to an empty collection and the list renders no rows. The property is `Rows`.
2. **Both interface methods take a `CancellationToken` last, defaulted.** `IAdminListService` and `IAdminListColumnProvider` end every method with `CancellationToken cancellationToken = default`. Pass `HttpContext.RequestAborted` from controllers. `GetColumnsAsync` takes the optional `data` bag before it, so name the argument (`cancellationToken: HttpContext.RequestAborted`) when the page passes no data.
3. **Keep the hidden `submit.Filter` button first in the form.** It is what Enter in the search box triggers. A visible Go button reuses the same name.
4. **Client-side search needs per-row attributes.** Set them on the row shape, not in the layout: `rowShape.Classes.Add("item")` and `rowShape.Attributes["data-filter-value"] = ...`. Both column layouts render a row shape's `Classes` and `Attributes` on its `<tr>` or row element.
5. **Bulk actions rely on names, not markup.** Keep `#select-all` and inputs named `itemIds`, and the `list-management` script keeps working in any layout.
6. **Render row actions through `AdminListActions`.** In a row template use `@await DisplayAsync(await New.AdminListActions(Row: Model))` rather than hand-writing the button group, so the configured actions layout (Buttons or Menu) applies everywhere.
7. **Responsive behaviour is CSS, not Razor.** The stacking below 48rem is a container query on the list in `_admin-list.scss`. Do not add viewport-based Bootstrap classes such as `d-md-none` to the layouts; the sidebar makes the viewport a poor proxy for the list width.
8. **Per-type row templates do not apply in column layouts.** `Content-BlogPost.SummaryAdmin.cshtml` is only used by the `List` layout. Customize a column with `AdminListCell-{ListName}-{Column}.cshtml` instead.

## References

- `references/worked-example.md` — the full Indexes page: driver, columns, controller and view.
- `src/docs/reference/modules/Admin/README.md` — user-facing documentation of the layouts and settings.
- `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminList*.cshtml` — the layout, cell, actions and toolbar templates.
- `src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_admin-list.scss` — layout CSS and the container queries.
