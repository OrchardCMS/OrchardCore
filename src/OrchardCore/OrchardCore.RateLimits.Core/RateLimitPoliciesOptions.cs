using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits;

/// <summary>
/// Holds a snapshot of the enabled rate-limit policies of the tenant, which is refreshed whenever
/// the enabled policies change, so that the tenant limiter applies them without being rebuilt.
/// </summary>
public sealed class RateLimitPoliciesOptions
{
    /// <summary>
    /// Gets or sets the enabled policies.
    /// </summary>
    public IReadOnlyCollection<RateLimitPolicy> EnabledPolicies { get; set; } = [];
}
