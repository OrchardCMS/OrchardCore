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
    /// <returns>The number of messages that were successfully sent to the user.</returns>
    Task<int> SendAsync(object notify, INotificationMessage message);
}
