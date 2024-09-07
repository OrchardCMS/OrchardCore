namespace OrchardCore.Notifications;

/// <summary>
/// Represents a contract for notification events.
/// </summary>
public interface INotificationEvents
{
    /// <summary>
    /// Occurs before the notification is created.
    /// </summary>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    Task CreatingAsync(NotificationContext context);

    /// <summary>
    /// Occurs after the notification is created.
    /// </summary>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    Task CreatedAsync(NotificationContext context);

    /// <summary>
    /// Occurs before the notification is sent.
    /// </summary>
    /// <param name="provider">The <see cref="INotificationMethodProvider"/>.</param>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    Task SendingAsync(INotificationMethodProvider provider, NotificationContext context);

    /// <summary>
    /// Occurs after the notification is sent.
    /// </summary>
    /// <param name="provider">The <see cref="INotificationMethodProvider"/>.</param>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    Task SentAsync(INotificationMethodProvider provider, NotificationContext context);

    /// <summary>
    /// Occurs when the notification is failed.
    /// </summary>
    /// <param name="provider">The <see cref="INotificationMethodProvider"/>.</param>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    Task FailedAsync(INotificationMethodProvider provider, NotificationContext context);

    /// <summary>
    /// Occurs before the notification is sent.
    /// </summary>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    Task SendingAsync(NotificationContext context);

    /// <summary>
    /// Occurs after the notification is sent.
    /// </summary>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    Task SentAsync(NotificationContext context);
}
