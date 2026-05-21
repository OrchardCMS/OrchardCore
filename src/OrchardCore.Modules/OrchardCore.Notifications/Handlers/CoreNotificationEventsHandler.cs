using OrchardCore.Entities;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Services;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Handlers;

public class CoreNotificationEventsHandler : NotificationEventsHandler
{
    /// <summary>
    /// Populates the notification entity with the recipient user id and body content before it is stored.
    /// </summary>
    /// <param name="context">The notification context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public override Task CreatingAsync(NotificationContext context, CancellationToken cancellationToken = default)
    {
        if (context.Notify is User user)
        {
            context.Notification.UserId = user.UserId;
        }

        var bodyPart = context.Notification.GetOrCreate<NotificationBodyInfo>();

        bodyPart.TextBody = context.NotificationMessage.TextBody;
        bodyPart.HtmlBody = context.NotificationMessage.HtmlBody;

        context.Notification.Put(bodyPart);

        return Task.CompletedTask;
    }
}
