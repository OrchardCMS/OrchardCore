using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public static class StandardPermissions
{
    [Obsolete("This permission is deprecated and will be removed in future releases. Instead, consider changing the role type to 'Owner' for enhanced functionality.")]
    public static readonly Permission SiteOwner = new("SiteOwner", "Site Owners Permission", isSecurityCritical: true);
}
