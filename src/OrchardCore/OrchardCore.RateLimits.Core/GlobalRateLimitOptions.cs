namespace OrchardCore.RateLimits;

public sealed class GlobalRateLimitOptions
{
    public int PermitLimit { get; set; } = 150;

    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

    public int QueueLimit { get; set; }
}
