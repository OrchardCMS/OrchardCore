using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.RateLimits;

/// <summary>
/// Provides helper methods for creating common built-in rate-limit partition strategies.
/// </summary>
public static class RateLimitPartitionHelpers
{
    private const string UserPartitionKeyPrefix = "user:";

    /// <summary>
    /// Creates a sliding-window limiter partitioned by remote IP address.
    /// </summary>
    /// <param name="policyName">The logical policy name used in the partition key.</param>
    /// <param name="permitLimit">The number of requests allowed during the sliding window.</param>
    /// <returns>A partition factory that creates the sliding-window limiter.</returns>
    public static Func<HttpContext, RateLimitPartition<string>> CreateSlidingWindowPerIpPolicy(string policyName, int permitLimit)
    {
        return CreateSlidingWindowPolicy(policyName, permitLimit, GetRemoteIpAddress);
    }

    /// <summary>
    /// Creates a sliding-window limiter partitioned by the key that the specified selector returns for each request.
    /// </summary>
    /// <param name="policyName">The logical policy name used in the partition key.</param>
    /// <param name="permitLimit">The number of requests allowed during the sliding window.</param>
    /// <param name="partitionKeySelector">Returns the key of the client that the request is counted against, such as <see cref="GetClientPartitionKey(HttpContext)"/>.</param>
    /// <returns>A partition factory that creates the sliding-window limiter.</returns>
    public static Func<HttpContext, RateLimitPartition<string>> CreateSlidingWindowPolicy(string policyName, int permitLimit, Func<HttpContext, string> partitionKeySelector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);
        ArgumentNullException.ThrowIfNull(partitionKeySelector);

        return context => RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: CreatePartitionKey(policyName, partitionKeySelector(context)),
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
        return CreateFixedWindowPolicy(policyName, permitLimit, window, GetRemoteIpAddress);
    }

    /// <summary>
    /// Creates a fixed-window limiter partitioned by the key that the specified selector returns for each request.
    /// </summary>
    /// <param name="policyName">The logical policy name used in the partition key.</param>
    /// <param name="permitLimit">The number of requests allowed during the fixed window.</param>
    /// <param name="window">The duration of the fixed window.</param>
    /// <param name="partitionKeySelector">Returns the key of the client that the request is counted against, such as <see cref="GetClientPartitionKey(HttpContext)"/>.</param>
    /// <returns>A partition factory that creates the fixed-window limiter.</returns>
    public static Func<HttpContext, RateLimitPartition<string>> CreateFixedWindowPolicy(string policyName, int permitLimit, TimeSpan window, Func<HttpContext, string> partitionKeySelector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);
        ArgumentNullException.ThrowIfNull(partitionKeySelector);

        return context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: CreatePartitionKey(policyName, partitionKeySelector(context)),
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

    /// <summary>
    /// Gets the key of the client that a request is counted against: the signed-in user, otherwise the remote IP address.
    /// </summary>
    /// <remarks>
    /// Signed-in users get their own budget, so that many users sharing an IP address, such as the employees of an office
    /// behind a single gateway, don't share a single budget. Anonymous requests can only be told apart by their IP address.
    /// </remarks>
    /// <param name="context">The current HTTP request context.</param>
    /// <returns>The partition key of the signed-in user, or the remote IP address for anonymous requests.</returns>
    public static string GetClientPartitionKey(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                return GetUserPartitionKey(userId);
            }
        }

        return GetRemoteIpAddress(context);
    }

    /// <summary>
    /// Gets the partition key of the specified user, which is the same key that <see cref="GetClientPartitionKey(HttpContext)"/>
    /// returns for the requests of that user once signed in.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>The partition key of the user.</returns>
    public static string GetUserPartitionKey(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        return UserPartitionKeyPrefix + userId;
    }

    private static string CreatePartitionKey(string policyName, string clientKey)
    {
        return $"{policyName}:{clientKey}";
    }
}
