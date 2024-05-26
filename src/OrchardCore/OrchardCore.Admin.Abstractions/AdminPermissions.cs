using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public class AdminPermissions
{
    // This was moved to the abstractions class since it need to access it from other modules.
    public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", "Access admin panel");
}
