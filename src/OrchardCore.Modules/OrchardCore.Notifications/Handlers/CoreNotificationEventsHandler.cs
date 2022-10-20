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

        if (context.NotificationMessage is INotificationContentMessage contentMessage)
        {
            var contentInfo = new NotificationContentInfo()
            {
                ContentItemId = contentMessage.ContentItemId,
                ContentOwnerId = contentMessage.ContentOwnerId,
                ContentType = contentMessage.ContentType,
                LinkType = contentMessage.LinkType,
            };

            if (contentMessage.LinkType == NotificationLinkType.Custom)
            {
                contentInfo.CustomUrl = contentMessage.CustomUrl;
            }

            context.Notification.Put(contentInfo);
        }

        if (context.NotificationMessage is INotificationBodyMessage nm)
        {
            var bodyPart = context.Notification.As<NotificationBodyInfo>();

            bodyPart.IsHtmlBody = nm.IsHtmlBody;
            bodyPart.Body = nm.Body;

            context.Notification.Put(bodyPart);
        }

        return Task.CompletedTask;
    }
}
