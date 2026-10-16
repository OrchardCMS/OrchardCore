# Worked example: the Indexes admin page

The Indexes page uses standard row display drivers, view-defined columns and the shared `AdminList` shape.

## Row display driver

`OrchardCore.Indexing/Drivers/IndexProfileDisplayDriver.cs` reuses shape types with display-specific locations:

```csharp
return CombineAsync(
    View("IndexProfile_Checkbox_SummaryAdmin", indexProfile)
        .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
        .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
    View("IndexProfile_Fields_SummaryAdmin", indexProfile)
        .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
        .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
    View("IndexProfile_DefaultTags_SummaryAdmin", indexProfile)
        .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
        .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Source:5"),
    View("IndexProfile_DefaultMeta_SummaryAdmin", indexProfile)
        .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
        .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Modified:5"),
    View("IndexProfile_Buttons_SummaryAdmin", indexProfile)
        .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
    View("IndexProfile_ActionsMenuItems_SummaryAdmin", indexProfile)
        .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
        .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5"));
```

The row template renders its zones for `List`; `Grid` renders the column zones directly.
The same shape types and templates are reused, but only the selected display type is built.

## Controller

The controller supplies queried `IndexProfile` records, not model/shape pairs:

```csharp
viewModel.List = new AdminListContext<IndexProfile>
{
    Source = viewModel.Models,
    Pager = viewModel.Pager,
};
```

`IndexProfileDisplayDriver` sets the row's optional `item` class and `data-filter-value` search attribute
on `context.Shape` for both admin display types. The list invokes the display manager after resolving
its layout; the controller has no row-rendering dependencies.

## Rendering view

`Views/Admin/Index.cshtml` declares the columns and presentation before displaying the list:

```cshtml
@addTagHelper *, OrchardCore.Admin.Abstractions
<admin-list name="Indexes" context="Model.List" bulk-actions="Model.Options.BulkActions"
            empty-message="@T["<strong>Nothing here!</strong> There are no indexes at the moment."]">
    <admin-list-list item-css-class="list-group-item" />
    <admin-list-grid>
        <admin-list-column type="Select" />
        <admin-list-column name="Name" title="@T["Name"]" position="20" />
        <admin-list-column name="Source" title="@T["Source"]" position="30" width="auto" />
        <admin-list-column name="Modified" title="@T["Last modified"]" position="40" width="auto" />
        <admin-list-column type="Actions" title="@T["Actions"]" />
    </admin-list-grid>
</admin-list>
```

The layout declarations configure each representation without choosing it. The tag helpers set the standard
shape's properties before display. Direct `New.AdminList(Source: Model.Models, ...)` and ordinary shape
rendering remain supported with C# column declarations. A simple listing can instead supply
`name`, `items` and `pager` directly to the tag without a context.

By default, each column renders its named row zone. `Zones` can explicitly render several zones in one cell.
Other features can add columns by implementing `IAdminListColumnsProvider` and checking
`context.ListName == "Indexes"`. They add cell shapes through ordinary row display drivers.
Themes can move a shape to a different column with `placement.json`,
e.g. `"displayType": "SummaryAdminGrid", "place": "Name:5"`.
