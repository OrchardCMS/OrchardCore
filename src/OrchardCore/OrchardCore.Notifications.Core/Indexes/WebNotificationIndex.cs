using System;
using System.Text.RegularExpressions;
using YesSql.Indexes;

namespace OrchardCore.Notifications.Indexes;

public class WebNotificationIndex : MapIndex
{
    public string NotificationId { get; set; }

    public string UserId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// The Content column should only be used to search the content of a notification.
    /// </summary>
    public string Content { get; set; }
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
                var content = webNotification.Subject ?? String.Empty;

                if (!String.IsNullOrEmpty(webNotification.Body))
                {
                    content += $" {webNotification.Body}";
                }

                content = StripHTML(content);

                if (content.Length > NotificationConstants.WebNotificationIndexContentLength)
                {
                    content = content[..NotificationConstants.WebNotificationIndexContentLength];
                }

                return new WebNotificationIndex()
                {
                    NotificationId = webNotification.NotificationId,
                    UserId = webNotification.UserId,
                    IsRead = webNotification.IsRead,
                    ReadAtUtc = webNotification.ReadAtUtc,
                    CreatedAtUtc = webNotification.CreatedUtc,
                    Content = content,
                };
            });
    }

    private static readonly Regex HtmlRegex = new("<.*?>", RegexOptions.Compiled);

    public static string StripHTML(string html)
    {
        return HtmlRegex.Replace(html, String.Empty);
    }
}
