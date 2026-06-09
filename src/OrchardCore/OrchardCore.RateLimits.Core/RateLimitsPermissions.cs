using OrchardCore.Security.Permissions;

namespace OrchardCore.RateLimits.Core;

public sealed class RateLimitsPermissions
{
    public static readonly Permission ManageRateLimits = new("ManageRateLimits", "Manage Rate Limits");
}
