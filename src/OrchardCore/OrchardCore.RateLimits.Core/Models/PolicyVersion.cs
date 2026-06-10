namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Identifies which stored version of a rate-limit policy to query.
/// </summary>
public enum PolicyVersion
{
    /// <summary>
    /// The editable draft version of a policy.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The published version of a policy that is enforced at runtime.
    /// </summary>
    Published = 1,
}
