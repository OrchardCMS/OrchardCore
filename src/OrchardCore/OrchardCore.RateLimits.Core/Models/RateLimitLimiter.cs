using OrchardCore.Entities;

namespace OrchardCore.RateLimits.Models;

public sealed class RateLimitLimiter : Entity
{
    public string Id { get; set; }

    public string Source { get; set; }
}
