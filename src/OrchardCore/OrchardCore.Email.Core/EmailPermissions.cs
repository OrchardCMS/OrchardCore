using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email;

public static class EmailPermissions
{
    public static readonly Permission ManageEmailSettings = new("ManageEmailSettings", LocalizationSource.Create("Manage Email Settings", typeof(EmailPermissions)));
}
