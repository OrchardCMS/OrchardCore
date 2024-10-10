using System.Security.Claims;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public static class StandardPermissions
{
    [Obsolete("This permission is deprecated and will be removed in future releases. Instead, consider adding users to the system administrator role.")]
    public static readonly Permission SiteOwner = new("SiteOwner", "Site Owners Permission", isSecurityCritical: true);

    /// <summary>
    /// This claim is assigned by the system during the login process if the user belongs to the Administrator role.
    /// </summary>
    public static readonly Claim SiteOwnerClaim = new("OrchardCorePermissions", "all-permissions");
}
