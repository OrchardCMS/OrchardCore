using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public static class AdminPermissions
{
    // This was moved to the abstractions class since it needs to be accessed from other modules.
    public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", "Access admin panel");
}
