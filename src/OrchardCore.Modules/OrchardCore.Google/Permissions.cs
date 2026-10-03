using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

public static class Permissions
{
    public static readonly Permission ManageGoogleAuthentication
        = new("ManageGoogleAuthentication", new LocalizationSource("Manage Google Authentication settings", typeof(Permissions)));

    public static readonly Permission ManageGoogleAnalytics
        = new("ManageGoogleAnalytics", new LocalizationSource("Manage Google Analytics settings", typeof(Permissions)));

    public static readonly Permission ManageGoogleTagManager
        = new("ManageGoogleTagManager", new LocalizationSource("Manage Google Tag Manager settings", typeof(Permissions)));
}
