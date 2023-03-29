using System;

namespace OrchardCore.Notifications;

public class NotificationContext
{
    public Notification Notification { get; set; }

    public INotificationMessage NotificationMessage { get; }

    public object Notify { get; }

    public NotificationContext(INotificationMessage notificationMessage, object notify)
    {
        NotificationMessage = notificationMessage ?? throw new ArgumentNullException(nameof(notificationMessage));
        Notify = notify ?? throw new ArgumentNullException(nameof(notify));
    }
}
