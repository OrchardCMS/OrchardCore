using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Roles.ViewModels;

namespace OrchardCore.Roles.Drivers;

/// <summary>
/// Builds the row of the roles admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <c>Roles</c> renders.
/// </summary>
public sealed class RoleEntryDisplayDriver : DisplayDriver<RoleEntry>
{
    public override Task<IDisplayResult> DisplayAsync(RoleEntry entry, BuildDisplayContext context)
    {
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape rowShape)
        {
            rowShape.Classes.Add("item");
            rowShape.Attributes["data-filter-value"] = entry.Name?.ToLowerInvariant() ?? string.Empty;
        }

        return CombineAsync(
            View("RoleEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("RoleEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:5"),
            View("RoleEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Kind:5"),
            View("RoleEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }
}
