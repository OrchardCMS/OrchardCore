using System.Text.Json.Nodes;

namespace OrchardCore.RateLimits.Endpoints;

/// <summary>Writable policy metadata and request matching. Status and limiters use separate operations.</summary>
public sealed class RateLimitPolicyInput
{
    /// <summary>Gets or sets the unique tenant policy name.</summary>
    public string Name { get; set; }
    /// <summary>Gets or sets the optional description.</summary>
    public string Description { get; set; }
    /// <summary>Gets or sets Global, Endpoint, or Group.</summary>
    public string Scope { get; set; }
    /// <summary>Gets or sets the absolute request path prefix for Endpoint scope.</summary>
    public string Path { get; set; }
    /// <summary>Gets or sets the group name for Group scope.</summary>
    public string GroupName { get; set; }
}

/// <summary>Safe policy state without ownership identifiers or arbitrary extension properties.</summary>
public sealed class RateLimitPolicyResponse
{
    /// <summary>Gets the stable policy identifier.</summary>
    public string PolicyId { get; init; }
    /// <summary>Gets policy metadata and matching settings.</summary>
    public RateLimitPolicyInput Definition { get; init; }
    /// <summary>Gets whether requests are limited by this policy.</summary>
    public bool IsEnabled { get; init; }
    /// <summary>Gets the last activation time.</summary>
    public DateTime? EnabledUtc { get; init; }
    /// <summary>Gets the configured limiters, with settings only for supported sources.</summary>
    public IReadOnlyList<RateLimitLimiterResponse> Limiters { get; init; }
}

/// <summary>Creates or replaces a limiter using a caller-selected identity for retries.</summary>
public sealed class RateLimitLimiterInput
{
    /// <summary>Gets or sets a nonempty stable identifier, at most 128 characters.</summary>
    public string Id { get; set; }
    /// <summary>Gets or sets the exact registered built-in source name.</summary>
    public string Source { get; set; }
    /// <summary>Gets or sets the complete source settings matching its discovered schema.</summary>
    public JsonObject Values { get; set; }
}

/// <summary>Describes one limiter without exposing uncontracted extension properties.</summary>
public sealed class RateLimitLimiterResponse
{
    /// <summary>Gets its stable identifier.</summary>
    public string Id { get; init; }
    /// <summary>Gets the source name.</summary>
    public string Source { get; init; }
    /// <summary>Gets whether this source supports remote configuration.</summary>
    public bool CanConfigure { get; init; }
    /// <summary>Gets supported source settings, or null for other sources.</summary>
    public JsonObject Values { get; init; }
}

/// <summary>Describes a built-in limiter's complete writable settings.</summary>
public sealed class RateLimitSourceResponse
{
    /// <summary>Gets the exact source name.</summary>
    public string Source { get; init; }
    /// <summary>Gets the JSON schema used for complete limiter settings.</summary>
    public JsonObject Schema { get; init; }
}
