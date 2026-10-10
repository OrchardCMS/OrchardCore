# Worked example: the Indexes admin page

The real conversion of `OrchardCore.Indexing`, from a hand-written `<ul class="list-group">` to the `AdminList` shape. Four files change and the page gains the List and Grid layouts plus the configured row-action layout.

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
<div class="col-auto d-flex justify-content-end ps-2">
    @* Rendered in the configured actions layout (Buttons, Menu, ...). *@
    @await DisplayAsync(await Factory.CreateAdminListActionsAsync((IShape)Model))
</div>
```

## 2. The list name and its columns

`IndexingAdminList.cs` publishes the name of the list:

```csharp
namespace OrchardCore.Indexing;

public static class IndexingAdminList
{
    public const string Name = "Indexes";
}
```

`IndexingAdminListColumnProvider.cs` declares its columns, each a set of zones of the row shape:

```csharp
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Indexing;

public sealed class IndexingAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public IndexingAdminListColumnProvider(IStringLocalizer<IndexingAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name takes the space left by the other columns.
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

        context.Columns.Add(new AdminListColumn
        {
            Name = "Modified",
            Position = "40",
            Title = S["Last modified"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
```

`Startup.cs` registers it for the list, so it is only created when the list is rendered:

```csharp
services.AddAdminListColumnProvider<IndexingAdminListColumnProvider>(IndexingAdminList.Name);
```

## 3. The controller

The page describes the list with an `AdminListContext` and hands it to `IAdminListFactory`. It has no options
editor shape, so the factory builds the generic `AdminListToolbar`: the item count from the rows and the pager,
the select-all checkbox and the bulk actions the page passes. The rows carry the attributes the client-side
search script reads.

`Controllers/AdminController.cs` (excerpt, after the view model is populated)

```csharp
// The rows carry the attributes used by the client-side search of the list-management script.
var rows = new List<IShape>(viewModel.Models.Count);

foreach (var entry in viewModel.Models)
{
    if (entry.Shape is Shape rowShape)
    {
        rowShape.Classes.Add("item");
        rowShape.Attributes["data-filter-value"] = entry.Model.Name?.ToLowerInvariant() ?? string.Empty;
    }

    rows.Add(entry.Shape);
}

// The AdminList shape renders the index profiles with the configured layout (List, Grid, ...).
viewModel.List = await adminListFactory.CreateAsync(new AdminListContext(IndexingAdminList.Name)
{
    Rows = rows,
    BulkActions = viewModel.Options.BulkActions,
    Pager = viewModel.Pager,
    ItemCssClass = "list-group-item",
    EmptyMessage = H["<strong>Nothing here!</strong> There are no indexes at the moment."],
}, HttpContext.RequestAborted);
```

The action signature injects the factory and keeps the existing parameters:

```csharp
[Admin("indexing", "IndexingIndex")]
public async Task<IActionResult> Index(
    IndexingEntityOptions options,
    PagerParameters pagerParameters,
    [FromServices] IOptions<PagerOptions> pagerOptions,
    [FromServices] IShapeFactory shapeFactory,
    [FromServices] IAdminListFactory adminListFactory)
```

The shape factory is still needed for the pager. A page that renders its search bar and its buttons through
the list passes them as `Search` and `Actions`, as the templates list does, so the layout places them.

The view model gains one property:

```csharp
public dynamic List { get; set; }
```

## 4. The view

`Views/Admin/Index.cshtml` keeps the form, the hidden submit buttons and its action bar with the search box and
the "Add index" button, and replaces the listing with one call: the toolbar, the rows and the pager come from
the shape.

```html
<form asp-action="Index" method="post" class="no-multisubmit bulk-select-list" data-selected-text="@T["selected"]">
    <input type="submit" name="submit.Filter" id="submitFilter" class="visually-hidden" />
    <input asp-for="Options.BulkAction" type="hidden" />
    <input type="submit" name="submit.BulkAction" class="visually-hidden" />

    @* ... the action bar card with the search box and the "Add index" button ... *@

    @* The index profiles, the toolbar and the pager are rendered by the AdminList shape using the configured layout (List, Grid, ...). *@
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
