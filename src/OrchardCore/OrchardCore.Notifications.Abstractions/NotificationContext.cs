namespace OrchardCore.Notifications;

public class NotificationContext
{
    public Notification Notification { get; set; }

    public INotificationMessage NotificationMessage { get; }

    public object Notify { get; }

    public NotificationContext(INotificationMessage notificationMessage, object notify)
    {
        NotificationMessage = notificationMessage;
        Notify = notify;
    }
}
