using Microsoft.Extensions.Localization;

namespace OrchardCore.Notifications;

/// <summary>
/// Represents a contract for a notification provider.
/// </summary>
public interface INotificationMethodProvider
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    /// <remarks>The name should be unique.</remarks>
    string Method { get; }

    /// <summary>
    /// Gets a localized name for the method.
    /// </summary>
    LocalizedString Name { get; }

    /// <summary>
    /// Attempts to send the given message to the given notifiable object.
    /// </summary>
    /// <param name="notify">The notifiable object.</param>
    /// <param name="message">The <see cref="INotificationMessage"/>.</param>
    /// <returns><c>true</c> when the message was successfully sent otherwise <c>false</c>.</returns>
    Task<bool> TrySendAsync(object notify, INotificationMessage message);
}
