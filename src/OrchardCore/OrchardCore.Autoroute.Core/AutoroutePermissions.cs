using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Autoroute;

public static class AutoroutePermissions
{
    public static readonly Permission SetHomepage = new("SetHomepage", LocalizationSource.Create("Set homepage.", typeof(AutoroutePermissions)));
}
