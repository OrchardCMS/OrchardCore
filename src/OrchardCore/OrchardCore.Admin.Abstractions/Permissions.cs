using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public class Permissions
{
    public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", "Access admin panel");
}
