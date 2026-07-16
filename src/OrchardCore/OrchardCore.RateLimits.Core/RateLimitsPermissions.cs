using OrchardCore.Security.Permissions;

namespace OrchardCore.RateLimits.Core;

/// <summary>
/// Defines permissions exposed by the Rate Limits feature.
/// </summary>
public sealed class RateLimitsPermissions
{
    /// <summary>
    /// Allows administrators to manage rate-limit policies and limiter settings.
    /// </summary>
    public static readonly Permission ManageRateLimits = new("ManageRateLimits", "Manage Rate Limits");
}
