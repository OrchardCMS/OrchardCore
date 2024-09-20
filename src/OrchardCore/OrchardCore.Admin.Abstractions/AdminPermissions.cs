using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public static class AdminPermissions
{
    public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", "Access admin panel");
}
