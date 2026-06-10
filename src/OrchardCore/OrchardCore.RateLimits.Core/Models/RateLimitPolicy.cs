using System.Text.Json.Serialization;

namespace OrchardCore.RateLimits.Models;

/// <summary>
/// Represents a draft or published rate-limit policy.
/// </summary>
public sealed class RateLimitPolicy
{
    /// <summary>
    /// Gets or sets the unique policy identifier.
    /// </summary>
    public string PolicyId { get; set; }

    /// <summary>
    /// Gets or sets the policy name shown to administrators.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional policy description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the owner identifier recorded for the policy.
    /// </summary>
    public string OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the author name recorded for the policy.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Gets or sets the policy scope that determines how requests are matched.
    /// </summary>
    public RateLimitPolicyScope Scope { get; set; }

    /// <summary>
    /// Gets or sets the request path prefix matched by endpoint policies.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the policy was last published.
    /// </summary>
    public DateTime? PublishedUtc { get; set; }

    /// <summary>
    /// Gets or sets the computed policy status for the current draft and published state.
    /// </summary>
    [JsonIgnore]
    public RateLimitPolicyStatus Status { get; set; }

    /// <summary>
    /// Gets the limiters applied when the policy matches a request.
    /// </summary>
    public List<RateLimitLimiter> Limiters { get; init; } = [];

}
