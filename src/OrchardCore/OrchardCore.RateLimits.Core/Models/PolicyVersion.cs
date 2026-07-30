namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Identifies which stored version of a rate-limit policy to query.
/// </summary>
public enum PolicyVersion
{
    /// <summary>
    /// The current editable version of a policy.
    /// </summary>
    Current = 0,

    /// <summary>
    /// The enabled version of a policy that is enforced at runtime.
    /// </summary>
    Enabled = 1,
}
