namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Describes the persisted state of a rate-limit policy.
/// </summary>
public enum RateLimitPolicyStatus
{
    /// <summary>
    /// The policy is stored but not currently enforced.
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// The policy is currently enforced at runtime.
    /// </summary>
    Enabled = 1,
}
