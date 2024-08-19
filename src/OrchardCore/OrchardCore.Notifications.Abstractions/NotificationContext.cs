namespace OrchardCore.Notifications;

/// <summary>
/// Represents a context for notification.
/// </summary>
public class NotificationContext
{
    /// <summary>
    /// Creates a new instance of <see cref="NotificationContext"/>.
    /// </summary>
    /// <param name="notificationMessage">The notification message.</param>
    /// <param name="notify">The notifiable object.</param>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="notificationMessage"/> is <c>null</c>,.</exception>
    public NotificationContext(INotificationMessage notificationMessage, object notify)
    {
        ArgumentNullException.ThrowIfNull(notificationMessage);
        ArgumentNullException.ThrowIfNull(notify);

        NotificationMessage = notificationMessage;
        Notify = notify;
    }

    /// <summary>
    /// Gets ot sets the notification.
    /// </summary>
    public Notification Notification { get; set; }

    /// <summary>
    /// Gets or sets the notification message.
    /// </summary>
    public INotificationMessage NotificationMessage { get; }

    /// <summary>
    /// Gets or sets the notifiable object.
    /// </summary>
    public object Notify { get; }
}
