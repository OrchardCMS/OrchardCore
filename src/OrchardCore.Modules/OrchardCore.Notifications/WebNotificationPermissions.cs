using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public class WebNotificationPermissions
{
    public static Permission ManageWebNotifications = new Permission("ManageWebNotifications", "Manage Web notifications");
}
