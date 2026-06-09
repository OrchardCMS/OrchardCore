namespace OrchardCore.RateLimits.Models;

public sealed class SlidingWindowRateLimiterData
{
    public int PermitLimit { get; set; }

    public int QueueLimit { get; set; }

    public int WindowSeconds { get; set; }

    public int SegmentsPerWindow { get; set; }
}
