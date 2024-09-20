using OrchardCore.Security.Permissions;

namespace OrchardCore.Autoroute;

public sealed class AutoroutePermissions
{
    public static readonly Permission SetHomepage = new("SetHomepage", "Set homepage.");
}
