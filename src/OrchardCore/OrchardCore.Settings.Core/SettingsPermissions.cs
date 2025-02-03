using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings;

public static class SettingsPermissions
{
    public static readonly Permission ManageSettings = new("ManageSettings", "Manage settings");

    // This permission is not exposed, it's just used for the APIs to generate/check custom ones.
    public static readonly Permission ManageGroupSettings = new("ManageResourceSettings", "Manage settings", new[] { ManageSettings });
}
