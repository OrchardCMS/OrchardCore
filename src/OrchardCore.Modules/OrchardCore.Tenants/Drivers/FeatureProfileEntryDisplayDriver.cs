using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Drivers;

/// <summary>
/// Builds the row of the feature profiles admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="FeatureProfilesAdminList"/> renders.
/// </summary>
public sealed class FeatureProfileEntryDisplayDriver : DisplayDriver<FeatureProfileEntry>
{
    public override Task<IDisplayResult> DisplayAsync(FeatureProfileEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("FeatureProfileEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("FeatureProfileEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("FeatureProfileEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
