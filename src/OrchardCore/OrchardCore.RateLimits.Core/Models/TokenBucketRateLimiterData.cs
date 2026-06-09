using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits.Models;

public sealed class TokenBucketRateLimiterData
{
    public int TokenLimit { get; set; }

    public int QueueLimit { get; set; }

    public int TokensPerPeriod { get; set; }

    public int ReplenishmentPeriodSeconds { get; set; }

    public QueueProcessingOrder QueueProcessingOrder { get; set; }
}
