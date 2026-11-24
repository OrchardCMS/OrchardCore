using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Themes.TheAdmin.Drivers;

/// <summary>
/// The rows of the list of the schemes, in both admin list layouts.
/// </summary>
internal sealed class AdminSchemeDisplayDriver : DisplayDriver<AdminSchemeEntry>
{
    public override IDisplayResult Display(AdminSchemeEntry entry, BuildDisplayContext context)
    {
        if (context.DisplayType is not (OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid))
        {
            return null;
        }

        if (context.Shape is Shape row)
        {
            row.Classes.Add("item");
            row.Attributes["data-filter-value"] = entry.Scheme.Name?.ToLowerInvariant() ?? string.Empty;
        }

        return Combine(
            View("AdminScheme_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("AdminScheme_Swatch_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Colors:5"),
            View("AdminScheme_Buttons_SummaryAdmin", entry).Location("Actions:5"),
            // The default scheme can't be deleted, nor made the default again: its row has no menu of actions.
            View("AdminScheme_ActionsMenu_SummaryAdmin", entry).Location("ActionsMenu:10").RenderWhen(() => Task.FromResult(!entry.IsDefault)));
    }
}
