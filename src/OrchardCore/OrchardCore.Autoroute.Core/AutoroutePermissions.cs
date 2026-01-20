using OrchardCore.Security.Permissions;

namespace OrchardCore.Autoroute;

public static class AutoroutePermissions
{
    public static readonly Permission SetHomepage = new("SetHomepage", "Set homepage.");
}
