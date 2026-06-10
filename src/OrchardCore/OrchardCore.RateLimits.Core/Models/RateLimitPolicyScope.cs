namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Defines how a rate-limit policy matches incoming requests.
/// </summary>
public enum RateLimitPolicyScope
{
    /// <summary>
    /// Matches every tenant request.
    /// </summary>
    Global = 0,

    /// <summary>
    /// Matches requests whose path starts with the configured prefix.
    /// </summary>
    Endpoint = 1,
}
