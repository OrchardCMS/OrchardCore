using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits;

public static class RateLimitPartitionHelpers
{
    public static Func<HttpContext, RateLimitPartition<string>> CreateSlidingWindowPerIpPolicy(string policyName, int permitLimit)
    {
        return context => RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: CreatePartitionKey(policyName, context),
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
                AutoReplenishment = true,
            });
    }

    public static Func<HttpContext, RateLimitPartition<string>> CreateFixedWindowPerIpPolicy(string policyName, int permitLimit, TimeSpan window)
    {
        return context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: CreatePartitionKey(policyName, context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
                AutoReplenishment = true,
            });
    }

    public static string GetRemoteIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string CreatePartitionKey(string policyName, HttpContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        return $"{policyName}:{GetRemoteIpAddress(context)}";
    }
}
