using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Users;

internal static class RateLimiterPolicyHelpers
{
    internal static Func<HttpContext, RateLimitPartition<string>> CreateSlidingWindowPolicy(int permitLimit)
        => context => RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
                AutoReplenishment = true,
            });

    internal static Func<HttpContext, RateLimitPartition<string>> CreateFixedWindowPolicy(int permitLimit, TimeSpan window)
        => context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
                AutoReplenishment = true,
            });
}
