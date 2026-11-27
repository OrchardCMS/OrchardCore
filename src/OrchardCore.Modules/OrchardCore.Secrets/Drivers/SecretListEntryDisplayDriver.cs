using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers;

/// <summary>
/// Builds the row of the list of the secrets, in both layouts of the list: each shape is placed in a zone of the
/// List row (<c>SummaryAdmin</c>) and in the column of the Grid (<c>SummaryAdminGrid</c>).
/// </summary>
internal sealed class SecretListEntryDisplayDriver : DisplayDriver<SecretListEntry>
{
    public override Task<IDisplayResult> DisplayAsync(SecretListEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("SecretListEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1")
                .RenderWhen(() => Task.FromResult(entry.CanManage)),
            View("SecretListEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("SecretListEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:5")
                .RenderWhen(() => Task.FromResult(!string.IsNullOrEmpty(entry.Description))),
            View("SecretListEntry_Type_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Type:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Type:5"),
            View("SecretListEntry_Store_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Store:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Store:5"),
            View("SecretListEntry_Expiration_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Expiration:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Expiration:5"),
            View("SecretListEntry_Updated_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Updated:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Updated:5")
                .RenderWhen(() => Task.FromResult(entry.UpdatedUtc.HasValue)),
            View("SecretListEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
                .RenderWhen(() => Task.FromResult(entry.CanManage))
        );
    }
}
