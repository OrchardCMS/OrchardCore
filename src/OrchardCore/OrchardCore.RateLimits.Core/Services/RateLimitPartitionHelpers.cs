using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.RateLimits;

/// <summary>
/// Provides helper methods for creating common built-in rate-limit partition strategies.
/// </summary>
public static class RateLimitPartitionHelpers
{
    /// <summary>
    /// Creates a sliding-window limiter partitioned by remote IP address.
    /// </summary>
    /// <param name="policyName">The logical policy name used in the partition key.</param>
    /// <param name="permitLimit">The number of requests allowed during the sliding window.</param>
    /// <returns>A partition factory that creates the sliding-window limiter.</returns>
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

    /// <summary>
    /// Creates a fixed-window limiter partitioned by remote IP address.
    /// </summary>
    /// <param name="policyName">The logical policy name used in the partition key.</param>
    /// <param name="permitLimit">The number of requests allowed during the fixed window.</param>
    /// <param name="window">The duration of the fixed window.</param>
    /// <returns>A partition factory that creates the fixed-window limiter.</returns>
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

    /// <summary>
    /// Gets the remote IP address used for request partitioning.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <returns>The remote IP address, or <c>unknown</c> when it cannot be resolved.</returns>
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
