# Worked example: the Indexes admin page

The real conversion of `OrchardCore.Indexing`, from a hand-written `<ul class="list-group">` to the `AdminList` shape. Four files change and the page gains the List, Table and Grid layouts plus the configured row-action layout.

## 1. The row shape gains a `Checkbox` zone

The selection checkbox used to be inline markup in the container view. It becomes a shape so the column layouts can put it in its own cell.

`Views/IndexProfile.Checkbox.SummaryAdmin.cshtml`

```html
@using OrchardCore.DisplayManagement.Views
@using OrchardCore.Indexing.Models

@model ShapeViewModel<IndexProfile>

<div class="form-check">
    <input type="checkbox" class="form-check-input" value="@Model.Value.Id" name="itemIds" id="itemIds-@Model.Value.Id">
    <label class="form-check-label" for="itemIds-@Model.Value.Id">&nbsp;</label>
</div>
```

`Drivers/IndexProfileDisplayDriver.cs`

```csharp
public override Task<IDisplayResult> DisplayAsync(IndexProfile indexProfile, BuildDisplayContext context)
{
    return CombineAsync(
        View("IndexProfile_Checkbox_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
        View("IndexProfile_Fields_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
        View("IndexProfile_Buttons_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),
        View("IndexProfile_DefaultTags_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
        View("IndexProfile_DefaultMeta_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5"),
        View("IndexProfile_ActionsMenuItems_SummaryAdmin", indexProfile)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
    );
}
```

The row template renders the checkbox zone and delegates the actions:

`Views/IndexProfile.SummaryAdmin.cshtml` (excerpt)

```html
@if (Model.Checkbox != null)
{
    <div class="selectors-container d-flex me-2">
        @await DisplayAsync(Model.Checkbox)
    </div>
}
...
<div class="col-lg-auto col-12 d-flex justify-content-end">
    @* Rendered in the configured actions layout (Buttons, Menu, ...). *@
    @await DisplayAsync(await New.AdminListActions(Row: Model))
</div>
```

## 2. The list name and its columns

`IndexingAdminList.cs`

```csharp
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Indexing;

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
            Name = "Modified",
            Position = "40",
            Title = S["Last modified"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        },
        new()
        {
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

## 3. The controller

The page has no options editor shape, so it builds the generic `AdminListToolbar` and passes it as `Toolbar`. The rows carry the attributes the client-side search script reads.

`Controllers/AdminController.cs` (excerpt, after the view model is populated)

```csharp
// The rows carry the attributes used by the client-side search of the list-management script.
var rows = new List<object>(viewModel.Models.Count);

foreach (var entry in viewModel.Models)
{
    if (entry.Shape is Shape rowShape)
    {
        rowShape.Classes.Add("item");
        rowShape.Attributes["data-filter-value"] = entry.Model.Name?.ToLowerInvariant() ?? string.Empty;
    }

    rows.Add(entry.Shape);
}

var toolbar = await shapeFactory.CreateAsync("AdminListToolbar", Arguments.From(new
{
    ItemsCount = viewModel.Models.Count,
    TotalItemCount = result.Count,
    StartIndex = viewModel.Models.Count > 0 ? pager.GetStartIndex() + 1 : 0,
    EndIndex = pager.GetStartIndex() + viewModel.Models.Count,
    BulkActions = viewModel.Options.BulkActions,
}));

viewModel.List = await shapeFactory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
{
    Name = IndexingAdminList.Name,
    Layout = await adminListService.GetLayoutAsync(IndexingAdminList.Name, cancellationToken: HttpContext.RequestAborted),
    Columns = await adminListService.GetColumnsAsync(IndexingAdminList.Name, IndexingAdminList.GetDefaultColumns(S), HttpContext.RequestAborted),
    Rows = rows,
    Toolbar = toolbar,
    Pager = viewModel.Pager,
    ItemCssClass = "list-group-item",
    EmptyMessage = H["<strong>Nothing here!</strong> There are no indexes at the moment."],
}));
```

The action signature injects the service and keeps the existing parameters:

```csharp
[Admin("indexing", "IndexingIndex")]
public async Task<IActionResult> Index(
    IndexingEntityOptions options,
    PagerParameters pagerParameters,
    [FromServices] IOptions<PagerOptions> pagerOptions,
    [FromServices] IShapeFactory shapeFactory,
    [FromServices] IAdminListService adminListService)
```

The view model gains one property:

```csharp
public dynamic List { get; set; }
```

## 4. The view

`Views/Admin/Index.cshtml` keeps the form, the hidden submit buttons and the action bar, and replaces the whole list block with one call.

```html
<form asp-action="Index" method="post" class="no-multisubmit" data-list-management data-client-side-search="true" data-selected-label="@T["selected"]">
    <input type="submit" name="submit.Filter" id="submitFilter" class="visually-hidden" />
    <input asp-for="Options.BulkAction" type="hidden" />
    <input type="submit" name="submit.BulkAction" class="visually-hidden" />

    <div class="card text-bg-theme mb-3 position-sticky action-bar">
        <div class="card-body">
            <div class="row gx-2">
                <div class="col">
                    <div class="input-group has-search">
                        <i class="fa-solid fa-search form-control-feedback" aria-hidden="true"></i>
                        <input id="search-box" asp-for="Options.Search" class="form-control" placeholder="@T["Search"]" type="search" autofocus autocomplete="off" />
                        <button type="submit" name="submit.Filter" class="btn btn-outline-secondary" title="@T["Search"]">@T["Go"]</button>
                    </div>
                </div>
                <div class="col-auto">
                    <button type="button" class="btn btn-secondary create" data-bs-toggle="modal" data-bs-target="#modalAddIndex">@T["Add index"]</button>
                </div>
            </div>
        </div>
    </div>

    @* The index profiles, the toolbar and the pager are rendered by the AdminList shape. *@
    @await DisplayAsync(Model.List)

    <div id="list-alert" class="alert alert-info my-3 d-none text-center" role="alert">
        @T["Your search returned no results."]
    </div>
</form>
```

## What was deleted

- The `<ul class="list-group with-checkbox">` block, the item count and select-all markup, and the bulk actions dropdown, all now in `AdminListToolbar`.
- The per-row `<li>` with its inline checkbox and `data-filter-value`, now the row shape plus its `Classes` and `Attributes`.
- The empty-state `<li>`, now the `EmptyMessage` property.

The pager was previously built but never rendered; `AdminList` renders it, so the page gained working paging and the page size selector for free.
