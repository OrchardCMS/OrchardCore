namespace OrchardCore.Notifications;

/// <summary>
/// Represents a contract for accessing the <see cref="INotificationMethodProvider"/>.
/// </summary>
public interface INotificationMethodProviderAccessor
{
    /// <summary>
    /// Gets the registered notification method providers.
    /// </summary>
    /// <param name="notify">The notifiable object.</param>
    /// <returns>A list of <see cref="INotificationMethodProvider"/>.</returns>
    Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(object notify);

    /// <summary>
    /// Gets the notification method providers to send the given message with, honoring the preferences
    /// the notifiable object has for the type of the message.
    /// </summary>
    /// <param name="notify">The notifiable object.</param>
    /// <param name="message">The message to be sent.</param>
    /// <returns>A list of <see cref="INotificationMethodProvider"/>.</returns>
    Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(object notify, INotificationMessage message)
        => GetProvidersAsync(notify);
}
