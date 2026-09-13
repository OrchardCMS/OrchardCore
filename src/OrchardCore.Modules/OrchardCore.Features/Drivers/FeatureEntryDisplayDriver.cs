using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Features.Models;

namespace OrchardCore.Features.Drivers;

/// <summary>
/// Builds the row of the features admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="FeaturesAdminList"/> renders.
/// </summary>
public sealed class FeatureEntryDisplayDriver : DisplayDriver<FeatureEntry>
{
    public override IDisplayResult Display(FeatureEntry entry, BuildDisplayContext context)
    {
        return Combine(
            View("FeatureEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .RenderWhen(() => Task.FromResult(entry.IsSelectable)),
            View("FeatureEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("FeatureEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("FeatureEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("FeatureEntry_DefaultMeta_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5"),
            View("FeatureEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
