using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits;

/// <summary>
/// Represents a read-only group-based limiter registered in code.
/// </summary>
public sealed class GroupRateLimit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GroupRateLimit"/> class.
    /// </summary>
    /// <param name="groupName">The group name to protect.</param>
    /// <param name="partitioner">The partition factory that creates the limiter.</param>
    public GroupRateLimit(string groupName, Func<HttpContext, RateLimitPartition<string>> partitioner)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentNullException.ThrowIfNull(partitioner);

        GroupName = groupName.Trim();
        Partitioner = partitioner;
    }

    /// <summary>
    /// Gets the group name that this limiter matches.
    /// </summary>
    public string GroupName { get; }

    /// <summary>
    /// Gets the partition factory used to create the limiter for a matching request.
    /// </summary>
    public Func<HttpContext, RateLimitPartition<string>> Partitioner { get; }
}
