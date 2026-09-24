using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Autoroute;

public static class AutoroutePermissions
{
    public static readonly Permission SetHomepage = new("SetHomepage", LocalizedString.Create("Set homepage.", typeof(AutoroutePermissions)));
}
