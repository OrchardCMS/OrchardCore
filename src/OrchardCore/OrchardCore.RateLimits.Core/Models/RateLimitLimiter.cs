using OrchardCore.Entities;

namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Represents a stored limiter definition attached to a rate-limit policy.
/// </summary>
public sealed class RateLimitLimiter : Entity
{
    /// <summary>
    /// Gets or sets the unique limiter identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the limiter source name that interprets the stored properties.
    /// </summary>
    public string Source { get; set; }

}
