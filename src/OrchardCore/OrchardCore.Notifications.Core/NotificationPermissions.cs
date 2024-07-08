using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public static class NotificationPermissions
{
    public readonly static Permission ManageNotifications = new("ManageNotifications", "Manage notifications");
}
