namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Stores settings for a fixed-window rate limiter.
/// </summary>
public sealed class FixedWindowRateLimiterData
{
    /// <summary>
    /// Gets or sets the number of requests allowed during each window.
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Gets or sets the number of requests that can wait in the queue.
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Gets or sets the fixed window duration in seconds.
    /// </summary>
    public int WindowSeconds { get; set; }
}
