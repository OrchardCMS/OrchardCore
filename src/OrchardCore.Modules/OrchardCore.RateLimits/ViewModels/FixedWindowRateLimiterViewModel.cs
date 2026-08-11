namespace OrchardCore.RateLimits.ViewModels;

public class FixedWindowRateLimiterViewModel
{
    public int PermitLimit { get; set; }

    public int QueueLimit { get; set; }

    public int WindowSeconds { get; set; }
}
