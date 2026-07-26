namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Stores settings for a sliding-window rate limiter.
/// </summary>
public sealed class SlidingWindowRateLimiterData
{
    /// <summary>
    /// Gets or sets the number of requests allowed during the full window.
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Gets or sets the number of requests that can wait in the queue.
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Gets or sets the sliding window duration in seconds.
    /// </summary>
    public int WindowSeconds { get; set; }

    /// <summary>
    /// Gets or sets the number of overlapping segments used for the window.
    /// </summary>
    public int SegmentsPerWindow { get; set; }
}
