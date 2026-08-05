using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Email;

public static class EmailPermissions
{
    public static readonly Permission ManageEmailSettings = new("ManageEmailSettings", "Manage Email Settings", null, [new Permission("ManageCustomSettings_email", "", [SettingsPermissions.ManageGroupSettings])]);
}
