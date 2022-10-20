using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Notifications;

public interface INotificationMethodProvider
{
    /// <summary>
    /// Unique name for the provider
    /// </summary>
    string Method { get; }

    /// <summary>
    /// A localized name for the method.
    /// </summary>
    LocalizedString Name { get; }

    /// <summary>
    /// Attempts to sent the given message to the given notifiable object
    /// </summary>
    /// <param name="notify"></param>
    /// <param name="message"></param>
    /// <returns>true when the message was successfuly sent otherwise false</returns>
    Task<bool> TrySendAsync(object notify, INotificationMessage message);
}
