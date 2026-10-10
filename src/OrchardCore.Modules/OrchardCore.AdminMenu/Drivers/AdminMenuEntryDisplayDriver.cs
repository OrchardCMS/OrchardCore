using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AdminMenu.Drivers;

/// <summary>
/// Builds the row of the admin menus list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="AdminMenusAdminList"/> renders.
/// </summary>
public sealed class AdminMenuEntryDisplayDriver : DisplayDriver<AdminMenuEntry>
{
    public override Task<IDisplayResult> DisplayAsync(AdminMenuEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("AdminMenuEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("AdminMenuEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("AdminMenuEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("AdminMenuEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),
            View("AdminMenuEntry_ActionsMenuItems_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
        );
    }
}
