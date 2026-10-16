using OrchardCore.DisplayManagement.Shapes;
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
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape rowShape)
        {
            var feature = entry.Feature;
            rowShape.Attributes["data-filter-value"] = $"{feature.Descriptor.Name} {feature.Descriptor.Id} {feature.Descriptor.Description}";
            rowShape.Attributes["data-is-on-demand"] = feature.Descriptor.EnabledByDependencyOnly.ToString().ToLowerInvariant();
            rowShape.Attributes["data-is-always-enabled"] = entry.IsAlwaysEnabled.ToString().ToLowerInvariant();
            rowShape.Attributes["data-is-enabled"] = feature.IsEnabled.ToString().ToLowerInvariant();
        }

        return Combine(
            View("FeatureEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1")
                .RenderWhen(() => Task.FromResult(entry.IsSelectable)),
            View("FeatureEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("FeatureEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:5"),
            View("FeatureEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Dependencies:5"),
            View("FeatureEntry_DefaultMeta_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Status:5"),
            View("FeatureEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }
}
