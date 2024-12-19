using OrchardCore.Security.Permissions;

namespace OrchardCore.Google;

public static class Permissions
{
    public static readonly Permission ManageGoogleAuthentication
        = new("ManageGoogleAuthentication", "Manage Google Authentication settings");

    public static readonly Permission ManageGoogleAnalytics
        = new("ManageGoogleAnalytics", "Manage Google Analytics settings");

    public static readonly Permission ManageGoogleTagManager
        = new("ManageGoogleTagManager", "Manage Google Tag Manager settings");
}
