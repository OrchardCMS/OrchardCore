using OrchardCore.Data.Documents;

namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Persists draft and published rate-limit policies for a tenant.
/// </summary>
public sealed class RateLimitPolicyDocument : Document
{
    /// <summary>
    /// Gets the stored draft policies.
    /// </summary>
    public List<RateLimitPolicy> DraftPolicies { get; init; } = [];

    /// <summary>
    /// Gets the stored published policies.
    /// </summary>
    public List<RateLimitPolicy> PublishedPolicies { get; init; } = [];
}
