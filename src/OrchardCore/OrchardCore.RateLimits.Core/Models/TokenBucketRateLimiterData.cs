using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Stores settings for a token-bucket rate limiter.
/// </summary>
public sealed class TokenBucketRateLimiterData
{
    /// <summary>
    /// Gets or sets the maximum number of tokens that can accumulate in the bucket.
    /// </summary>
    public int TokenLimit { get; set; }

    /// <summary>
    /// Gets or sets the number of requests that can wait in the queue.
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens restored per replenishment period.
    /// </summary>
    public int TokensPerPeriod { get; set; }

    /// <summary>
    /// Gets or sets the replenishment period in seconds.
    /// </summary>
    public int ReplenishmentPeriodSeconds { get; set; }

    /// <summary>
    /// Gets or sets the order used when dequeuing waiting requests.
    /// </summary>
    public QueueProcessingOrder QueueProcessingOrder { get; set; }
}
