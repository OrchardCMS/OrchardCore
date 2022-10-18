namespace OrchardCore.Notifications;

public class NotificationConstants
{
    public const string NotificationCollection = "Notification";

    /// <summary>
    /// If this number is increased after the site is setup, you must alter the <see cref="WebNotification" /> using migration
    /// </summary>
    public const int WebNotificationIndexContentLength = 2500;
}
