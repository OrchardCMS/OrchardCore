using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Notifications;

public static class NotificationPermissions
{
    public static readonly Permission ManageNotifications = new("ManageNotifications", LocalizedString.Create("Manage notifications", typeof(NotificationPermissions)));
}
