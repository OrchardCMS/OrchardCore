namespace OrchardCore.RateLimits.Models;

public sealed class RateLimitPolicyEntry
{
    public string PolicyId { get; set; }

    public RateLimitPolicy Draft { get; set; }

    public RateLimitPolicy Published { get; set; }

    public DateTime? PublishedUtc { get; set; }
}
