using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.Drivers;

/// <summary>
/// Builds the row of the layers admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="LayersAdminList"/> renders.
/// </summary>
public sealed class LayerDisplayDriver : DisplayDriver<Layer>
{
    public override IDisplayResult Display(Layer layer, BuildDisplayContext context)
    {
        return Combine(
            View("Layer_Checkbox_SummaryAdmin", layer)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("Layer_Fields_SummaryAdmin", layer)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("Layer_Description_SummaryAdmin", layer)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("Layer_Buttons_SummaryAdmin", layer)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
