namespace OrchardCore.RateLimits;

/// <summary>
/// Provides rate-limit group names for endpoint metadata.
/// </summary>
public interface IRateLimitGroupMetadata
{
    /// <summary>
    /// Gets the rate-limit groups attached to an endpoint.
    /// </summary>
    IReadOnlyList<string> GroupNames { get; }
}
