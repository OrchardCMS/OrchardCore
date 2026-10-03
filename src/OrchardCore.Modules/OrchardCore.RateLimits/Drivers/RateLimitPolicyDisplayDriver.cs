using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Drivers;

public sealed class RateLimitPolicyDisplayDriver : DisplayDriver<RateLimitPolicy>
{
    public override IDisplayResult Display(RateLimitPolicy model, BuildDisplayContext context)
        => View("RateLimitPolicy_ActionsMenuItems_SummaryAdmin", model)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5");
}
