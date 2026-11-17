using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Themes.TheAdmin.Drivers;

/// <summary>
/// The rows of the list of schemes, in both admin list layouts.
/// </summary>
public sealed class AdminSchemeDisplayDriver : DisplayDriver<AdminScheme>
{
    public override IDisplayResult Display(AdminScheme scheme, BuildDisplayContext context)
    {
        if (context.DisplayType is not (OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid))
        {
            return null;
        }

        if (context.Shape is Shape row)
        {
            row.Classes.Add("item");
            row.Attributes["data-filter-value"] = scheme.Name?.ToLowerInvariant() ?? string.Empty;
        }

        return Combine(
            View("AdminScheme_Fields_SummaryAdmin", scheme)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("AdminScheme_Swatch_SummaryAdmin", scheme)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Colors:5"),
            View("AdminScheme_Buttons_SummaryAdmin", scheme).Location("Actions:5"),
            View("AdminScheme_ActionsMenu_SummaryAdmin", scheme).Location("ActionsMenu:10"));
    }
}
