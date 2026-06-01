using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure;

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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the notification method sent the message successfully.</returns>
    Task<Result> TrySendAsync(object notify, INotificationMessage message, CancellationToken cancellationToken = default);
}
