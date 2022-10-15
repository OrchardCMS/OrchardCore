using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public class WebNotificationPermission
{
    public static Permission ManageOwnNotifications = new Permission("ManageOwnNotifications", "Manage user's own notifications");
}
