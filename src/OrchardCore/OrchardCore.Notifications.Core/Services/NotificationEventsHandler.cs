namespace OrchardCore.Notifications.Services;

public class NotificationEventsHandler : INotificationEvents
{
    /// <summary>
    /// Called before a notification entity is created.
    /// </summary>
    /// <param name="context">The notification context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task CreatingAsync(NotificationContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called after a notification entity is created.
    /// </summary>
    /// <param name="context">The notification context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task CreatedAsync(NotificationContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called before a notification method sends the notification.
    /// </summary>
    /// <param name="provider">The notification method provider.</param>
    /// <param name="context">The notification context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task SendingAsync(INotificationMethodProvider provider, NotificationContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called after a notification method sends the notification successfully.
    /// </summary>
    /// <param name="provider">The notification method provider.</param>
    /// <param name="context">The notification context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task SentAsync(INotificationMethodProvider provider, NotificationContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called after a notification method fails to send the notification.
    /// </summary>
    /// <param name="provider">The notification method provider.</param>
    /// <param name="context">The notification context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task FailedAsync(INotificationMethodProvider provider, NotificationContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called after the notification has been sent through all available notification methods.
    /// </summary>
    /// <param name="context">The notification context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task SentAsync(NotificationContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called before the notification is sent through the available notification methods.
    /// </summary>
    /// <param name="context">The notification context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task SendingAsync(NotificationContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
