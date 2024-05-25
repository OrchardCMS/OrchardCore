using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public class AdminPermissions
{
    public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", "Access admin panel");
}
