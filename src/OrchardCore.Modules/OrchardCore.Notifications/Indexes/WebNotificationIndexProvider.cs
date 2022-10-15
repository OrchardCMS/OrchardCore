using System;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.Notifications.Models;
using YesSql.Indexes;

namespace OrchardCore.Notifications.Indexes;

public class WebNotificationMessageIndexProvider : IndexProvider<ContentItem>
{
    private readonly IClock _clock;

    public WebNotificationMessageIndexProvider(IClock clock)
    {
        _clock = clock;
    }

    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<WebNotificationMessageIndex>()
            .Map(contentItem =>
            {
                if (!String.Equals(contentItem.ContentType, NotificationConstants.WebNotificationMessageContentType))
                {
                    return null;
                }
                var infoPart = contentItem.As<WebNotificationMessagePart>();

                if (infoPart == null)
                {
                    return null;
                }

                var index = new WebNotificationMessageIndex()
                {
                    ContentItemId = contentItem.ContentItemId,
                    UserId = contentItem.Owner,
                    IsRead = infoPart.IsRead,
                    ReadAtUtc = infoPart.ReadAtUtc,
                    CreatedAtUtc = contentItem.CreatedUtc ?? _clock.UtcNow,
                };

                if (!String.IsNullOrEmpty(infoPart.Subject))
                {
                    index.Subject = infoPart.Subject[..Math.Min(infoPart.Subject.Length, 255)];
                }

                if (!String.IsNullOrEmpty(infoPart.Body))
                {
                    index.Body = infoPart.Body[..Math.Min(infoPart.Body.Length, 255)];
                }

                return index;
            });
    }
}


