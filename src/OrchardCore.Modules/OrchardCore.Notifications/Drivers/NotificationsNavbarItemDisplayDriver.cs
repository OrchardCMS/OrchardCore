using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Queries;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotificationsNavbarItemDisplayDriver : DisplayDriver<NotificationsNavbarItem>
{
    private readonly NotificationOptions _notificationOptions;
    private readonly YesSql.ISession _session;

    public NotificationsNavbarItemDisplayDriver(
        IOptions<NotificationOptions> notificationOptions,
        YesSql.ISession session)
    {
        _notificationOptions = notificationOptions.Value;
        _session = session;
    }

    public override IDisplayResult Display(NotificationsNavbarItem navbarItem, BuildDisplayContext context)
    {
        var result = Initialize<UserNotificationNavbarViewModel>("UserNotificationNavbar", async model =>
        {
            var query = new QueryTopUnreadNotificationsByUserId(navbarItem.UserId, _notificationOptions.TotalUnreadNotifications + 1);
            var notifications = (await _session.ExecuteQuery(query, collection: NotificationConstants.NotificationCollection).ListAsync()).ToList();

            model.Notifications = notifications;
            model.MaxVisibleNotifications = _notificationOptions.TotalUnreadNotifications;
            model.TotalUnread = notifications.Count;
        }).Location("Detail", "Content:5");

        return result;
    }
}
