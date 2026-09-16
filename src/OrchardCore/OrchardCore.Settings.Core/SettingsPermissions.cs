using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings;

public static class SettingsPermissions
{
    public static readonly Permission ManageSettings = new("ManageSettings", "Manage settings");

    /// <summary>
    /// Grants permission to manage the built-in General settings group.
    /// </summary>
    public static readonly Permission ManageGeneralSettings = new("ManageGeneralSettings", "Manage general settings", [ManageSettings]);

    /// <summary>
    /// Grants permission to manage the built-in Debugging settings group.
    /// </summary>
    public static readonly Permission ManageDebuggingSettings = new("ManageDebuggingSettings", "Manage debugging settings", [ManageSettings]);

    // This permission is not exposed, it's just used for the APIs to generate/check custom ones.
    public static readonly Permission ManageGroupSettings = new("ManageResourceSettings", "Manage settings", new[] { ManageSettings });
}
