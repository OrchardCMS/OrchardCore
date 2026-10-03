using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email;

public static class EmailPermissions
{
    public static readonly Permission ManageEmailSettings = new("ManageEmailSettings", new LocalizationSource("Manage Email Settings", typeof(EmailPermissions)));
}
