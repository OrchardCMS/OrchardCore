using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public class NotificationPermissions
{
    public readonly static Permission ManageNotifications = new("ManageNotifications", "Manage notifications");
}
