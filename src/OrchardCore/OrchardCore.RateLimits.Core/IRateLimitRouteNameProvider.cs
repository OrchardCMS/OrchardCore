using OrchardCore.RateLimits.Services;

namespace OrchardCore.RateLimits.Core;

public interface IRateLimitRouteNameProvider
{
    IEnumerable<string> GetRouteNames();

    IEnumerable<RateLimitRouteInfo> GetRoutes();
}
