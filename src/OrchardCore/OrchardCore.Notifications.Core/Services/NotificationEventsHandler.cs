using System.Threading.Tasks;

namespace OrchardCore.Notifications.Services;

public class NotificationEventsHandler : INotificationEvents
{
    public virtual Task CreatingAsync(NotificationContext context) => Task.CompletedTask;

    public virtual Task CreatedAsync(NotificationContext context) => Task.CompletedTask;

    public virtual Task SendingAsync(INotificationMethodProvider provider, NotificationContext context) => Task.CompletedTask;

    public virtual Task SentAsync(INotificationMethodProvider provider, NotificationContext context) => Task.CompletedTask;

    public virtual Task FailedAsync(INotificationMethodProvider provider, NotificationContext context) => Task.CompletedTask;

    public virtual Task SentAsync(NotificationContext context) => Task.CompletedTask;

    public virtual Task SendingAsync(NotificationContext context) => Task.CompletedTask;
}
