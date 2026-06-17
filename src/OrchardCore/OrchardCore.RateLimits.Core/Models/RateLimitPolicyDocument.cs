using OrchardCore.Data.Documents;

namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Persists rate-limit policies for a tenant.
/// </summary>
public sealed class RateLimitPolicyDocument : Document
{
    /// <summary>
    /// Gets the stored policies.
    /// </summary>
    public List<RateLimitPolicy> Policies { get; init; } = [];
}
