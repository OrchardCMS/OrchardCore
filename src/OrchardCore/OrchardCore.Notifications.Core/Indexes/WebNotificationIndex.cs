using System;
using YesSql.Indexes;

namespace OrchardCore.Notifications.Indexes;

public class WebNotificationIndex : MapIndex
{
    public string ContentItemId { get; set; }

    public string UserId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    // public string Subject { get; set; }
    // public string Body { get; set; }
    // public bool IsHtmlBody { get; set; }
}

public class WebNotificationIndexProvider : IndexProvider<WebNotification>
{
    public WebNotificationIndexProvider()
    {
        CollectionName = NotificationConstants.NotificationCollection;
    }
    public override void Describe(DescribeContext<WebNotification> context)
    {
        context.For<WebNotificationIndex>()
            .Map(webNotification =>
            {
                return new WebNotificationIndex()
                {
                    ContentItemId = webNotification.NotificationId,
                    UserId = webNotification.UserId,
                    IsRead = webNotification.IsRead,
                    ReadAtUtc = webNotification.ReadAtUtc,
                    CreatedAtUtc = webNotification.CreatedUtc,
                };
            });
    }
}
