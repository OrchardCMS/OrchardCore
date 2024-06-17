using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public static class StandardPermissions
{
    public static readonly Permission SiteOwner = new("SiteOwner", "Site Owners Permission", isSecurityCritical: true);
}
