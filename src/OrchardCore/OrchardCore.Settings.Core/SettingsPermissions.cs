using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings;

public static class SettingsPermissions
{
    public static readonly Permission ManageSettings = new("ManageSettings", new LocalizationSource("Manage settings", typeof(SettingsPermissions)));

    /// <summary>
    /// Grants permission to manage the built-in General settings group.
    /// </summary>
    public static readonly Permission ManageGeneralSettings = new("ManageGeneralSettings", new LocalizationSource("Manage general settings", typeof(SettingsPermissions)), [ManageSettings]);

    /// <summary>
    /// Grants permission to manage the built-in Debugging settings group.
    /// </summary>
    public static readonly Permission ManageDebuggingSettings = new("ManageDebuggingSettings", new LocalizationSource("Manage debugging settings", typeof(SettingsPermissions)), [ManageSettings]);

    // This permission is not exposed, it's just used for the APIs to generate/check custom ones.
    public static readonly Permission ManageGroupSettings = new("ManageResourceSettings", new LocalizationSource("Manage settings", typeof(SettingsPermissions)), new[] { ManageSettings });
}
