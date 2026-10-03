using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public static class NotificationPermissions
{
    public static readonly Permission ManageNotifications = new("ManageNotifications", new LocalizationSource("Manage notifications", typeof(NotificationPermissions)));
}
