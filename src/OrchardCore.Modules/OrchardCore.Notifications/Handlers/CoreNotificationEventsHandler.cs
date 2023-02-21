using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Services;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Handlers;

public class CoreNotificationEventsHandler : NotificationEventsHandler
{
    public override Task CreatingAsync(NotificationContext context)
    {
        var user = context.Notify as User;

        if (user != null)
        {
            context.Notification.UserId = user.UserId;
        }

        var bodyPart = context.Notification.As<NotificationBodyInfo>();

        bodyPart.Summary = context.NotificationMessage.Summary;
        bodyPart.TextBody = context.NotificationMessage.TextBody;
        bodyPart.HtmlBody = context.NotificationMessage.HtmlBody;

        context.Notification.Put(bodyPart);

        return Task.CompletedTask;
    }
}
