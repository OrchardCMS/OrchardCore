using System;
using System.Threading.Tasks;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Services;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Handlers;

public class CoreNotificationEventsHandler : NotificationEventsHandler
{
    public override Task CreatingAsync(NotificationContext context)
    {
        if (context.User is User su)
        {
            context.Notification.UserId = su.UserId;
        }

        if (context.NotificationMessage is ContentNotificationMessage contentMessage)
        {
            if (!String.IsNullOrEmpty(contentMessage.ContentItemId))
            {
                context.Notification.ContentItemId = contentMessage.ContentItemId;
            }
            else if (!String.IsNullOrWhiteSpace(contentMessage.Url))
            {
                context.Notification.Url = contentMessage.Url;
            }
        }

        if (context.NotificationMessage is HtmlNotificationMessage nm)
        {
            context.Notification.IsHtmlBody = nm.BodyContainsHtml;
        }

        return Task.CompletedTask;
    }
}
