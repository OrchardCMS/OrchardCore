using System.Threading.Tasks;
using OrchardCore.Users;

namespace OrchardCore.Notifications;

public interface INotificationManager
{
    /// <summary>
    /// Attempts to sent the given message to the given user.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <returns>The number of messages that were successfully sent to the user.</returns>
    Task<int> SendAsync(IUser user, INotificationMessage message);
}
