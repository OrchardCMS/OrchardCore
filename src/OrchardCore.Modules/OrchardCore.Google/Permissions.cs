using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

public static class Permissions
{
    public static readonly Permission ManageGoogleAuthentication
        = new("ManageGoogleAuthentication", LocalizationSource.Create("Manage Google Authentication settings", typeof(Permissions)));

    public static readonly Permission ManageGoogleAnalytics
        = new("ManageGoogleAnalytics", LocalizationSource.Create("Manage Google Analytics settings", typeof(Permissions)));

    public static readonly Permission ManageGoogleTagManager
        = new("ManageGoogleTagManager", LocalizationSource.Create("Manage Google Tag Manager settings", typeof(Permissions)));
}
