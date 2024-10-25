using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public static class StandardPermissions
{
    [Obsolete("This permission is deprecated and will be removed in future releases. Instead, consider adding users to the system administrator role.")]
    public static readonly Permission SiteOwner = new("SiteOwner", "Site Owners Permission", isSecurityCritical: true);
}
