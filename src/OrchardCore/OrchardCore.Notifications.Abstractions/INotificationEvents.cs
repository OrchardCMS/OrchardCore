using System.Threading.Tasks;

namespace OrchardCore.Notifications;

public interface INotificationEvents
{
    Task CreatingAsync(NotificationContext context);

    Task CreatedAsync(NotificationContext context);

    Task SendingAsync(INotificationMethodProvider provider, NotificationContext context);

    Task SentAsync(INotificationMethodProvider provider, NotificationContext context);

    Task FailedAsync(INotificationMethodProvider provider, NotificationContext context);

    Task SendingAsync(NotificationContext context);

    Task SentAsync(NotificationContext context);
}
