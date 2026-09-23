using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

public static class Permissions
{
    public static readonly Permission ManageGoogleAuthentication
        = new("ManageGoogleAuthentication", LocalizedString.Create("Manage Google Authentication settings", typeof(Permissions)));

    public static readonly Permission ManageGoogleAnalytics
        = new("ManageGoogleAnalytics", LocalizedString.Create("Manage Google Analytics settings", typeof(Permissions)));

    public static readonly Permission ManageGoogleTagManager
        = new("ManageGoogleTagManager", LocalizedString.Create("Manage Google Tag Manager settings", typeof(Permissions)));
}
