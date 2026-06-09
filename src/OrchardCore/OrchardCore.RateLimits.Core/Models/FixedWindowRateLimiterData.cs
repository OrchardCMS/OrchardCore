namespace OrchardCore.RateLimits.Models;

public sealed class FixedWindowRateLimiterData
{
    public int PermitLimit { get; set; }

    public int QueueLimit { get; set; }

    public int WindowSeconds { get; set; }
}
