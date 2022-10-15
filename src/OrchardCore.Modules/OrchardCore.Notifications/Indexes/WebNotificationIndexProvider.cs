using System;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.Notifications.Models;
using YesSql.Indexes;

namespace OrchardCore.Notifications.Indexes;

public class WebNotificationIndexProvider : IndexProvider<ContentItem>
{
    private readonly IClock _clock;

    public WebNotificationIndexProvider(IClock clock)
    {
        _clock = clock;
    }

    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<WebNotificationIndex>()
            .Map(contentItem =>
            {
                if (!String.Equals(contentItem.ContentType, NotificationConstants.WebNotificationContentType))
                {
                    return null;
                }

                var infoPart = contentItem.As<WebNotificationPart>();

                return new WebNotificationIndex()
                {
                    ContentItemId = contentItem.ContentItemId,
                    UserId = contentItem.Owner,
                    IsRead = infoPart?.IsRead ?? false,
                    ReadAtUtc = infoPart?.ReadAtUtc,
                    CreatedAtUtc = contentItem.CreatedUtc ?? _clock.UtcNow,
                };
            });
    }
}


