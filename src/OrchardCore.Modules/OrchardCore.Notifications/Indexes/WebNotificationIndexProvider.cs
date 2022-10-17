using YesSql.Indexes;

namespace OrchardCore.Notifications.Indexes;

public class WebNotificationIndexProvider : IndexProvider<WebNotification>
{
    public WebNotificationIndexProvider()
    {
        CollectionName = WebNotification.Collection;
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
