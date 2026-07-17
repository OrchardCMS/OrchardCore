using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Stores settings for a concurrency rate limiter.
/// </summary>
public sealed class ConcurrencyRateLimiterData
{
    /// <summary>
    /// Gets or sets the maximum number of concurrent in-flight requests.
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Gets or sets the number of requests that can wait in the queue.
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Gets or sets the order used when dequeuing waiting requests.
    /// </summary>
    public QueueProcessingOrder QueueProcessingOrder { get; set; }
}
