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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the create callback finishes.</returns>
    Task CreatingAsync(NotificationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs after the notification is created.
    /// </summary>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the created callback finishes.</returns>
    Task CreatedAsync(NotificationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs before the notification is sent.
    /// </summary>
    /// <param name="provider">The <see cref="INotificationMethodProvider"/>.</param>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the provider sending callback finishes.</returns>
    Task SendingAsync(INotificationMethodProvider provider, NotificationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs after the notification is sent.
    /// </summary>
    /// <param name="provider">The <see cref="INotificationMethodProvider"/>.</param>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the provider sent callback finishes.</returns>
    Task SentAsync(INotificationMethodProvider provider, NotificationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs when the notification is failed.
    /// </summary>
    /// <param name="provider">The <see cref="INotificationMethodProvider"/>.</param>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the provider failure callback finishes.</returns>
    Task FailedAsync(INotificationMethodProvider provider, NotificationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs before the notification is sent.
    /// </summary>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the notification sending callback finishes.</returns>
    Task SendingAsync(NotificationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Occurs after the notification is sent.
    /// </summary>
    /// <param name="context">The <see cref="NotificationContext"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the notification sent callback finishes.</returns>
    Task SentAsync(NotificationContext context, CancellationToken cancellationToken = default);
}
