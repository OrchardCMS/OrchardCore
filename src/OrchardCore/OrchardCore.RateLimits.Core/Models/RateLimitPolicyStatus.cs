namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Describes the persisted state of a rate-limit policy.
/// </summary>
public enum RateLimitPolicyStatus
{
    /// <summary>
    /// The policy has a draft but no published version.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The policy has a published version and no draft changes.
    /// </summary>
    Published = 1,

    /// <summary>
    /// The policy has a published version and separate draft changes.
    /// </summary>
    PublishedWithDraft = 2,
}
