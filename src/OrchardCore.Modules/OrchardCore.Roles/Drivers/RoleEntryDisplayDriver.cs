using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Roles.ViewModels;

namespace OrchardCore.Roles.Drivers;

/// <summary>
/// Builds the row of the roles admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="RolesAdminList"/> renders.
/// </summary>
public sealed class RoleEntryDisplayDriver : DisplayDriver<RoleEntry>
{
    public override Task<IDisplayResult> DisplayAsync(RoleEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RoleEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("RoleEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("RoleEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("RoleEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
