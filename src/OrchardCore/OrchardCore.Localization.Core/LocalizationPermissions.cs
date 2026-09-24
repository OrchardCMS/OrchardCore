using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization;

public static class LocalizationPermissions
{
    public static readonly Permission ManageCultures = new("ManageCultures", LocalizedString.Create("Manage supported culture", typeof(LocalizationPermissions)));
}
