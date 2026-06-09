using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Drivers;

public sealed class RateLimitLimiterDisplayDriver : DisplayDriver<RateLimitLimiter>
{
    public override Task<IDisplayResult> DisplayAsync(RateLimitLimiter limiter, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RateLimitLimiter_Fields_SummaryAdmin", limiter).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("RateLimitLimiter_Buttons_SummaryAdmin", limiter).Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:1")
        );
    }
}
