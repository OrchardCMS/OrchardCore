using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.ViewModels;

public class RateLimitPolicyEntryViewModel
{
    public string PolicyId { get; set; }

    public RateLimitPolicy Policy { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string TargetDescription { get; set; }

    public RateLimitPolicyStatus Status { get; set; }

    public DateTime? EnabledUtc { get; set; }

    public bool IsEnabled { get; set; }
}
