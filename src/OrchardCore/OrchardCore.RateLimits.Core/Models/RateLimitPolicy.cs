namespace OrchardCore.RateLimits.Models;

public sealed class RateLimitPolicy
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string OwnerId { get; set; }

    public string Author { get; set; }

    public RateLimitPolicyScope Scope { get; set; }

    public string RouteName { get; set; }

    public string Path { get; set; }

    public List<RateLimitLimiter> Limiters { get; init; } = [];
}
