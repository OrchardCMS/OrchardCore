using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Placements.Models;

namespace OrchardCore.Placements.Drivers;

/// <summary>
/// Builds the row of the placements admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="PlacementsAdminList"/> renders.
/// </summary>
public sealed class ShapePlacementDisplayDriver : DisplayDriver<ShapePlacement>
{
    public override Task<IDisplayResult> DisplayAsync(ShapePlacement placement, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ShapePlacement_Checkbox_SummaryAdmin", placement)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("ShapePlacement_Fields_SummaryAdmin", placement)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("ShapePlacement_Buttons_SummaryAdmin", placement)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
