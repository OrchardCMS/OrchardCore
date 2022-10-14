using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Users;

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
    /// Attempts to sent the given message to the given user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <returns>true when the message was successfuly sent otherwise false</returns>
    Task<bool> TrySendAsync(IUser user, INotificationMessage message);
}
