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
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("RateLimitPolicy_Fields_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("RateLimitPolicy_DefaultTags_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("RateLimitPolicy_DefaultMeta_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5"),
            View("RateLimitPolicy_Buttons_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),
            View("RateLimitPolicy_ActionsMenuItems_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5"));
    }
}
