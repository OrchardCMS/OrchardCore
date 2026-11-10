using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public static class NotificationPermissions
{
    public static readonly Permission ManageNotifications = new("ManageNotifications", LocalizationSource.Create("Manage notifications", typeof(NotificationPermissions)));
}
