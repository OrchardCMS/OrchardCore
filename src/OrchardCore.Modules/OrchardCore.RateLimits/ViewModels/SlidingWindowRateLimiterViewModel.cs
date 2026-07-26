namespace OrchardCore.RateLimits.ViewModels;

public class SlidingWindowRateLimiterViewModel
{
    public int PermitLimit { get; set; }

    public int QueueLimit { get; set; }

    public int WindowSeconds { get; set; }

    public int SegmentsPerWindow { get; set; }
}
