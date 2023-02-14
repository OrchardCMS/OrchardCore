using System.Threading.Tasks;

namespace OrchardCore.Notifications;

public interface INotificationService
{
    /// <summary>
    /// Attempts to sent the given message to the given notifiable object.
    /// </summary>
    /// <param name="notify"></param>
    /// <param name="message"></param>
    /// <returns>The number of messages that were successfully sent to the user.</returns>
    Task<int> SendAsync(object notify, INotificationMessage message);
}
