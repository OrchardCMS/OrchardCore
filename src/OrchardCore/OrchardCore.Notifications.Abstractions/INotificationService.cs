namespace OrchardCore.Notifications;

/// <summary>
/// Contract for notification service.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Attempts to send the given message to the given notifiable object.
    /// </summary>
    /// <param name="notify">The notifiable object.</param>
    /// <param name="message">The message to be sent.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="NotificationSendResult"/> describing the outcome of the send operation across all notification methods.</returns>
    Task<NotificationSendResult> SendAsync(object notify, INotificationMessage message, CancellationToken cancellationToken = default);
}
