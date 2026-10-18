using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Drivers;

public sealed class RateLimitPolicyDisplayDriver : DisplayDriver<RateLimitPolicy>
{
    public override IDisplayResult Display(RateLimitPolicy model, BuildDisplayContext context)
    {
        return Combine(
            View("RateLimitPolicy_Checkbox_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
            View("RateLimitPolicy_Fields_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("RateLimitPolicy_Description_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:5"),
            View("RateLimitPolicy_DefaultTags_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Status:5"),
            View("RateLimitPolicy_DefaultMeta_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Enabled:5"),
            View("RateLimitPolicy_Buttons_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
            View("RateLimitPolicy_ActionsMenuItems_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5"));
    }
}
