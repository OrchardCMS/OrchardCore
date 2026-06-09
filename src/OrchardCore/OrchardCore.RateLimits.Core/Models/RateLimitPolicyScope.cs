namespace OrchardCore.RateLimits.Models;

public enum RateLimitPolicyScope
{
    Global = 0,
    Route = 1,
    Endpoint = 2,
}
