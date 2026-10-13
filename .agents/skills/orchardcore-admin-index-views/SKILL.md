---
name: orchardcore-admin-index-views
description: Builds OrchardCore admin index (list) pages with the AdminList shape — switchable List/Grid layouts, columns mapped to row zones, the search bar, toolbar, pager and page size selector as parts of the shape, IAdminListFactory and AdminListContext<TModel>, column drivers (AdminListColumnDriver<TModel>), row actions, per-list template alternates, and client-side search. Use when creating or converting an admin listing page (Index.cshtml, *AdminList.cshtml), adding or reordering its columns, overriding the template of one list, or changing how its rows and actions render.
---

# OrchardCore Admin Index Views

This skill guides you through building an admin index page (a listing screen such as **Manage Content**, **Users**, or **Indexes**) on top of the reusable `AdminList` shape, so the page inherits the site-wide layout and row-action settings instead of hard-coding its own markup.

## Mental model

An admin index page has three layers. Only the first is written per page.

| Layer | Owner | What it does |
|-------|-------|--------------|
| The page's view | The module | A `<form>` with the antiforgery token and the hidden filter and bulk-action inputs, then the list. |
| `AdminList` shape | `OrchardCore.Admin` | Renders the whole listing in the configured **layout**: search bar, page buttons, toolbar, rows, pager and page size selector. |
| Row shapes | Display drivers | One `SummaryAdmin` shape per item, its zones filled by drivers and `placement.json`. |

The layout only decides **how** the already-built rows are presented:

The layout comes from `AdminListOptions`, which binds the `OrchardCore:Admin:List` configuration section and is then overridden by the site settings. It follows the signal-backed options pattern, so consumers read `IOptionsMonitor<AdminListOptions>.CurrentValue` and the settings driver calls `IOptionsUpdateNotifier.RequestUpdate<AdminListOptions>()`. Never hard-code a layout name in a page: `IAdminListFactory` resolves it for the list, and a page only sets `AdminListContext.Layout` when it can be rendered one way alone.

- `List` renders each row shape whole, so the row template (`Content.SummaryAdmin.cshtml`) decides the look.
- `Grid` renders one **column** per `AdminListColumn`. A cell renders the **row zones** its column names, then the shapes the **column driver** built for that cell. The row template is not used.

Because a column is a list of zones plus what its driver builds, adding a column never means touching the layout templates — it means placing a shape in a zone, or building it in a column driver.

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

### Step 2: Name the list and declare its columns

One static class publishes the name of the list, so the controller, the tests and any column driver share it:

```csharp
public static class IndexingAdminList
{
    public const string Name = "Indexes";
}
```

The columns come from a column driver, an `AdminListColumnDriver<TModel>` registered for the items of the list,
the module's own included. Like a display driver, it builds shapes with `Initialize`, `Factory`, `Dynamic` or
`Combine` and places them by the name of a column. `Columns()` runs once per list and declares the columns (and
can return shapes for their headers); `Cells()` runs once per row, for the layouts with columns only.
`AdminListColumns.Select()` and `AdminListColumns.Actions(title)` are the selection and actions columns most
lists share:

```csharp
public sealed class IndexingAdminListColumnDriver : AdminListColumnDriver<IndexProfile>
{
    private readonly IStringLocalizer S;

    public IndexingAdminListColumnDriver(IStringLocalizer<IndexingAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // No width: this column takes the space left by the others.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Source",
            Position = "30",
            Title = S["Source"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        // Position "end" keeps the actions last even when a feature appends a column.
        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        // The titles of the columns are their headers.
        return null;
    }
}
```

```csharp
// Startup: registered for the items, so the driver is only created when a list of index profiles is rendered.
services.AddAdminListColumnDriver<IndexProfile, IndexingAdminListColumnDriver>();
```

A driver whose items are listed by one list only needs no list check. When several lists render the same
items, e.g. the content items of the Contents list and of a list part, the driver of each overrides
`CanHandleList(listName)`. A column name is unique in its list, `context.Columns` is keyed by it and throws on a
duplicate.

### Step 3: Build the `AdminList` shape in the controller

Describe the listing with an `AdminListContext<TModel>` and hand it to `IAdminListFactory`, injected per action
with `[FromServices]`. Each row is an `AdminListRow<TModel>`: the item and the row shape built for it, so the
column drivers can build cells from the item. The factory builds the columns, resolves the layout and, for a
list without an options editor, the toolbar:

```csharp
public async Task<IActionResult> Index(
    IndexingEntityOptions options,
    PagerParameters pagerParameters,
    [FromServices] IAdminListFactory adminListFactory)
{
    // ... query and pager, then each row: rows.Add(new(indexProfile, shape)) ...

    viewModel.List = await adminListFactory.CreateAsync(new AdminListContext<IndexProfile>(IndexingAdminList.Name)
    {
        Rows = rows,
        BulkActions = viewModel.Options.BulkActions,
        Pager = viewModel.Pager,
        ItemCssClass = "list-group-item",
        EmptyMessage = H["<strong>Nothing here!</strong> There are no indexes at the moment."],
    }, HttpContext.RequestAborted);

    return View(viewModel);
}
```

A page that renders its search bar and its buttons through the shape passes them too, as the templates list
does: `Search = await shapeFactory.CreateAsync("AdminListSearch", Arguments.From(new { Name = "Options.Search", Value = options.Search }))`
and `Actions = await shapeFactory.CreateAsync("TemplateCreateButton", ...)`, so the layout places them.

### Step 4: Render it

The view is the form and the list. The search bar, the buttons of the page, the toolbar, the rows, the pager and the page size selector all come from the shape, so a layout can place them:

```html
<form asp-action="Index" method="post" class="no-multisubmit bulk-select-list"
      data-checkbox-name="itemIds" data-bulk-action-name="Options.BulkAction" data-selected-text="@T["selected"]">
    <input type="submit" name="submit.Filter" id="submitFilter" class="visually-hidden" />
    <input asp-for="Options.BulkAction" type="hidden" />
    <input type="submit" name="submit.BulkAction" class="visually-hidden" />

    @await DisplayAsync(Model.List)
</form>

<script asp-src="~/OrchardCore.Indexing/Scripts/indexing-admin-index/indexing-admin-index.min.js"
        debug-src="~/OrchardCore.Indexing/Scripts/indexing-admin-index/indexing-admin-index.js" at="Foot" type="module"></script>
```

The selection and the client-side search are the shared components of `.scripts/bloom/components`
(`bulk-select-list`, `list-search-filter`), wired by one small module per page (`Assets/ts/*.ts`, built by
`yarn build --name <entry>`). The component reads the name of the row checkboxes from the root it is given, so
a list only has to keep the well-known ids `#select-all`, `#items`, `#selected-items` and `#actions`, which
`AdminListToolbar` renders.

The hidden `submit.Filter` button stays first: it is what Enter in the search box triggers. Buttons passed as `Actions` are a shape of their own, which keeps the route values and the localization in a template rather than in the controller:

```html
@* TemplateCreateButton.cshtml *@
@{
    var adminTemplates = Model.AdminTemplates != null && (bool)Model.AdminTemplates;
}

<a asp-action="Create" asp-controller="Template" asp-route-admintemplates="@adminTemplates" asp-route-returnUrl="@FullRequestPath" class="btn btn-secondary create" role="button">@T["Add Template"]</a>
```

A page with a filter dropdown before the search box adds it to the `Filters` zone of the `AdminListSearch` shape, which renders it inside the input group.

### Step 5: The toolbar

The strip above the rows comes from one of three places:

- **Nothing to do** — a list without an options editor gets the generic `AdminListToolbar` from the factory:
  the item count, taken from `Rows` and the `Pager` (its `Page`, `PageSize` and `TotalItemCount`), the
  select-all checkbox and a dropdown of `BulkActions`. `ShowSelectAll = false` keeps the count alone for rows
  that cannot be selected, and `ToolbarActions` renders a shape at its end, e.g. the filters of the list.
- **`Header`** — the options editor shape from `IDisplayManager<TOptions>.BuildEditorAsync()`. Its `Summary` zone renders on the left (count, select-all) and its `Actions` zone on the right (filters, bulk actions). Use this when the page already has an options editor, as Contents and Users do. The factory builds no toolbar then.
- **`Toolbar`** — a shape of your own, rendered as is, e.g. the rate limits pass one whose bulk actions carry their own warning. The factory builds no toolbar then.

A list with no toolbar at all, e.g. the recipes, sets `ShowToolbar = false`.

## Quick reference

### `AdminList` shape properties

| Property | Description |
|----------|-------------|
| `Name` | List name, e.g. `Contents`. Drives the `__{Name}` alternates and tells the column drivers which list they render. |
| `Layout` | `AdminListContext.Layout` when the page sets it, otherwise resolved by `IAdminListLayoutResolver.GetLayoutAsync()`. |
| `Columns` | Built by `IAdminListColumnsBuilder` from the column drivers of the items of the list. |
| `Rows` | The row shapes. **Never call this `Items`** — see Gotchas. |
| `Headers` | Layouts with columns only. An `IZoneHolding`, one zone per column, holding the header shapes the drivers built; an empty zone shows the `Title`. |
| `Cells` | Layouts with columns only. One `IZoneHolding` per row, in the order of `Rows`, one zone per column, holding the cell shapes the drivers built. |
| `Header` | Options editor shape (`Summary` + `Actions` zones). |
| `Toolbar` | Alternative to `Header`; a shape rendered as is. Built by the factory from `BulkActions`, `ShowSelectAll` and `ToolbarActions` when the page sets neither, unless `ShowToolbar` is false. |
| `Search` | Optional. The search bar, usually the `AdminListSearch` shape (`Name`, `Value`, `Placeholder`, `Id`, `SubmitName`, `Autofocus`, and a `Filters` zone before the input). |
| `Actions` | Optional. The buttons of the page, e.g. "Add", rendered beside the search. |
| `PageSize` | Optional. The page size selector, built with `PageSizeSelector.BuildOptions()`. Set `ShowPageSizeSelector = false` on the pager when you pass it. |
| `LayoutSelector` | Built for the list when the site lets a user choose their layout, so a page never passes it. Pass one to render your own instead. |
| `Pager` | The pager shape. It renders the page size selector itself unless `ShowPageSizeSelector` is false. |
| `RowsAttributes` | Optional. `IDictionary<string, string>` rendered on the element wrapping the rows in every layout, e.g. the id a sortable script needs. |
| `ItemCssClass` | Per-item classes in the `List` layout. |
| `EmptyMessage` | Message when there are no rows. |

### `AdminListColumn` properties

| Property | Description |
|----------|-------------|
| `Name` | Technical name. Drives `AdminListCell__{Name}` alternates and the `admin-list-column-{name}` class. |
| `Title` | Localized header. `null` for a column with no header, e.g. the checkbox. Still labels the stacked cells when a driver renders the header as a shape. |
| `Zones` | Row zones rendered in the cell, in order, before the shapes the drivers built for it. None for a column the drivers fill alone. |
| `Position` | Placement syntax (`10`, `25`, `end`). Sorted after every driver ran. |
| `Width` | Any CSS width, or `AdminListColumn.AutoWidth` to hug the content. No width means "share the rest". |
| `Alignment` | `Start`, `Center`, `End`. |
| `NoWrap` | Keeps the cell on one line. |
| `CssClass` | Extra classes on the header and the cells. |

### Alternates

| Shape | Alternates (least to most specific) |
|-------|--------------------------------------|
| `AdminList` | `AdminList__{Layout}`, `AdminList__{Name}`, `AdminList__{Name}__{Layout}` |
| `AdminListCell` | `AdminListCell__{Column}`, `AdminListCell__{ListName}__{Column}` — no layout variant, cells only exist in the layouts with columns |
| `AdminListActions` | `AdminListActions__{Layout}`, `AdminListActions__{ListName}`, `AdminListActions__{ListName}__{Layout}` — here `{Layout}` is `Buttons` or `Menu` |
| `AdminListToolbar` | `AdminListToolbar__{Layout}`, `AdminListToolbar__{ListName}`, `AdminListToolbar__{ListName}__{Layout}` |
| `AdminListSearch` | `AdminListSearch__{Layout}`, `AdminListSearch__{ListName}`, `AdminListSearch__{ListName}__{Layout}` |

An alternate that names a list but no layout wins over the layout alternate, so `AdminList-Users.cshtml` renders the users list in **every** layout. Name the layout, `AdminList-Users-Grid.cshtml`, to change one mode only.

### One sticky bar, one footer

The action bar of a layout sticks to the top of the page only when it carries the `Search`, so a page that
renders its own search bar never ends up with two bars covering each other. Everything else above the rows
belongs on the toolbar strip: pass a `Toolbar` (or a `Header`) rather than a second card of your own, and give
a list with nothing else to say the item count, which is what every other list shows there. The footer centres
the pager and puts the page size selector at its end.

### Letting a user choose a layout

With **Let users choose the layout of a list** on, `AdminList` builds a `LayoutSelector` of its own and the
layouts render it beside the item count. A click reloads with `?layout=Grid`, which
`IAdminListLayoutResolver.GetLayoutAsync` honours and remembers in a cookie, per list. `GetAvailableLayoutsAsync()`
returns what the site can render, and a layout the site does not have is ignored wherever it comes from.
Nothing is needed from a page: the pager and the page size selector already keep the query string.

A page rendering one list per group, e.g. the features or the recipes, turns the selector off on its lists with
`ShowLayoutSelector = false` and places one selector in its own header, from
`IAdminListLayoutResolver.GetLayoutOptionsAsync()`, so the page shows one selector however many lists it renders.

### Adding a layout — two files, no C#

`AdminList-Cards.cshtml` renders the list, and `AdminListLayout-Cards.Option.cshtml` makes it selectable
(and puts it in the user's selector; ship `AdminListLayoutSelectorItem-Cards.cshtml` to give its button an
icon):

```html
@{
    string current = Model.ListLayout;
}
<option value="Cards" selected="@(current == "Cards")">@T["Cards"]</option>
```

The settings page discovers layouts by scanning the shape table for `AdminListLayout_Option__*`, exactly as the content type editor discovers field editors from `{Field}_Option__*`. Row-action layouts work the same way with `AdminListActions_Option__*`.

### Adding a column from another feature

```csharp
public sealed class CultureColumnDriver : AdminListColumnDriver<ContentItem>
{
    private readonly IStringLocalizer S;

    public CultureColumnDriver(IStringLocalizer<CultureColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    // Only the content items list; without it, the driver adds its column to every list of content items.
    public override bool CanHandleList(string listName)
        => listName == ContentsAdminList.Name;

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        // Between "Title" (20) and "Type" (30), whatever order the drivers run in.
        context.Columns.Add(new AdminListColumn
        {
            Name = "Culture",
            Position = "25",
            Title = S["Culture"],
            Width = AdminListColumn.AutoWidth,
        });

        return null;
    }

    // The rows have no zone holding the culture, so the driver builds the cell from the item.
    public override IDisplayResult Cells(ContentItem contentItem, AdminListCellsContext context)
    {
        return Initialize<CultureViewModel>("ContentCulture_AdminListCell", model => model.Culture = contentItem.As<LocalizationPart>()?.Culture)
            .Location("Culture");
    }
}
```

```csharp
services.AddAdminListColumnDriver<ContentItem, CultureColumnDriver>();
```

A column can name the zones of the row it renders, which reuses the shapes the `SummaryAdmin` display drivers
built, or be filled by the cells its driver builds, or both: a cell renders the zones first. A shape placed in a
zone no column is named after is not rendered.

Every part of a list carries the name of the list, so each part can be overridden for one list alone:
`AdminList-{List}.cshtml`, `AdminList-{List}-{Layout}.cshtml`, `AdminListCell-{List}-{Column}.cshtml`,
`AdminListActions-{List}.cshtml`, `AdminListActions-{List}-{ActionsLayout}.cshtml`,
`AdminListToolbar-{List}.cshtml`, `AdminListToolbar-{List}-{Layout}.cshtml`, `AdminListSearch-{List}.cshtml`
and `AdminListSearch-{List}-{Layout}.cshtml`. The `AdminList` shape stamps its name and its layout on the
toolbar, the search bar, the pager, the page size selector and every row, so a row template renders
`AdminListActions` without passing a list name and still gets those alternates. `{Layout}` is the layout of
the list, except on `AdminListActions`, where it is `Buttons` or `Menu`.

`context.Find(name)` and `context.Remove(name)` alter existing columns. Never rely on the position of a column in the collection; use `Position`.

A driver often needs to know more than the name of the list: the content items list is the same list whether it shows every item or only the blog posts. The page fills `AdminListContext.Data`, and the driver reads it back from its context:

```csharp
var list = new AdminListContext<ContentItem>(ContentsAdminList.Name) { Rows = rows, Header = header, Pager = pagerShape };
list.Data[ContentsAdminList.ContentTypesKey] = new[] { "BlogPost" };

var listShape = await adminListFactory.CreateAsync(list, HttpContext.RequestAborted);
```

```csharp
if (context.TryGetData<string[]>(ContentsAdminList.ContentTypesKey, out var contentTypes) &&
    contentTypes.Contains("BlogPost"))
{
    context.Columns.Add(new AdminListColumn { Name = "Category", Position = "25", Title = S["Category"], Zones = ["Category"] });
}
```

`context.Data` is never null, and `TryGetData<T>` / `GetData<T>` fall back instead of throwing on a missing key or another type. A list documents the keys it fills: the content items list fills `ContentsAdminList.ContentTypesKey` and `ContentsAdminList.StereotypesKey` from its route, and leaves them out when the listing is not filtered.

### The shape of a row

Every shipped row follows the same structure, so the lists look alike. Copy it when converting a list:

```html
<div class="row g-0 align-items-center">
    <div class="col">
        <div class="title d-flex align-items-center">
            @* Handle, Checkbox, then <div class="summary d-flex flex-column flex-md-row list-item-search-text"> with Content, Tags, Meta *@
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

The actions sit beside the whole row, centred on it, and the description is inside the left column so it stops where the actions begin. The column holding the actions is `col-auto` at every width, so a narrow screen keeps them beside the row instead of dropping them onto a line of their own.

| Zone | Renders as | Never |
|------|-----------|-------|
| `Content` | The title, a plain link to the edit page | A heading (`<h5>`), which makes the row shout next to the other lists |
| `Tags`, `Meta` | `<span class="badge ta-badge fw-normal">` per fact, leading icon, `title` tooltip | Plain text or a `<span class="hint">` |
| `Description` | `<div class="admin-list-secondary">`, its own line under the title | A second full-width row, which runs under the actions |
| `Actions`, `ActionsMenu` | Through `AdminListActions` | A hand-written button group |

## Gotchas

1. **Do not name the rows property `Items`.** `Shape` already exposes `Items` (its child shapes), so `Model.Items` in the template silently resolves to an empty collection and the list renders no rows. The property is `Rows`.
2. **The factory takes a `CancellationToken` last, defaulted.** `IAdminListFactory.CreateAsync` ends with `CancellationToken cancellationToken = default`. Pass `HttpContext.RequestAborted` from controllers. Column drivers, like display drivers, take none.
3. **Keep the hidden `submit.Filter` button first in the form.** It is what Enter in the search box triggers. The visible search button, a magnifying glass at the end of the input group, reuses the same name. Never put the magnifying glass inside the input.
4. **Client-side search needs per-row attributes.** Set them on the row shape, not in the layout: `rowShape.Classes.Add("item")` and `rowShape.Attributes["data-filter-value"] = ...`. Every layout renders a row shape's `Classes` and `Attributes` on its row element.
5. **Bulk actions rely on names, not markup.** Keep `#select-all` and row checkboxes named consistently, and tell the `bulk-select-list` component the name with `data-checkbox-name`; it then works in any layout. A page that sorts its rows puts that class, and its `data-sort-url`, on the rows container through `RowsAttributes` (a `class` there joins the classes of the element), because the script sorts the element it is given.
6. **Render row actions through `AdminListActions`.** In a row template use `@await DisplayAsync(await Factory.CreateAdminListActionsAsync((IShape)Model))` rather than hand-writing the button group, so the configured actions layout (Buttons or Menu) applies everywhere.
7. **Responsive behaviour is CSS, not Razor.** The stacking below 48rem is a container query on the list in `_admin-list.scss`. Do not add viewport-based Bootstrap classes such as `d-md-none` to the layouts; the sidebar makes the viewport a poor proxy for the list width.
8. **Per-type row templates do not apply in column layouts.** `Content-BlogPost.SummaryAdmin.cshtml` is only used by the `List` layout. Customize a column with `AdminListCell-{ListName}-{Column}.cshtml` instead.
9. **Never wrap `ActionsMenu` items in `<li>`.** The dropdown sits inside the `<li>` of the row in the `List` layout, and the parser hoists a nested `<li>` out of it, taking the rest of the row with it: the menu renders empty and stray links appear under the list.
10. **Register the row driver in the feature that owns the list.** A driver registered in a neighbouring feature leaves every row empty on a site where that feature is off — the rows render, with no name, no badges and no buttons.
11. **A sortable list drags the rows, never their container.** A row of the `Grid` layout is a subgrid of the list, so it has a box to drag, but the element holding the rows is `display: contents` and measures as nothing. Mark that element with `RowsAttributes` and send SortableJS `oldDraggableIndex`/`newDraggableIndex`, which count only the rows, so the indexes are the same in every layout.
12. **A shape type is `typeof(TModel).Name`.** `DisplayManager<TModel>` uses it verbatim, so register the driver against a domain model named exactly what the shape should be called, never a `*ViewModel`.

## References

- `references/worked-example.md` — the full Indexes page: display driver, column driver, controller and view.
- `src/docs/reference/modules/Admin/README.md` — user-facing documentation of the layouts and settings.
- `src/OrchardCore.Modules/OrchardCore.Admin/Views/AdminList*.cshtml` — the layout, cell, actions and toolbar templates.
- `src/OrchardCore.Themes/TheAdmin/Assets/scss/components/_admin-list.scss` — layout CSS and the container queries.
- `src/OrchardCore/OrchardCore.Navigation.Core/PageSizeSelector.cs` — builds the options of a page size selector a list places itself.
- `test/OrchardCore.Tests/Modules/OrchardCore.Admin/AdminListDefinitionsTests.cs` — finds every list by reflection and checks the names and the columns, so a new list is covered without touching the test.
