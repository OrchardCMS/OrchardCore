using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization;

public static class LocalizationPermissions
{
    public static readonly Permission ManageCultures = new("ManageCultures", "Manage supported culture");
}
