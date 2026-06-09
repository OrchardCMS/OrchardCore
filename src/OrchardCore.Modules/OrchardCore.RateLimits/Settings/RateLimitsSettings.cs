namespace OrchardCore.RateLimits.Settings;

/// <summary>
/// Stores site-level settings for the Rate Limits feature.
/// </summary>
public sealed class RateLimitsSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the tenant-wide global limiter is enforced in addition to route-specific limits.
    /// </summary>
    public bool EnableGlobalRateLimiter { get; set; } = true;
}
