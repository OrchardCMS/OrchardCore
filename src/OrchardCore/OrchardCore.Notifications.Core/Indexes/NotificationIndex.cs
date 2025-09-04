using System.Text.RegularExpressions;
using OrchardCore.Entities;
using OrchardCore.Notifications.Models;
using YesSql.Indexes;

namespace OrchardCore.Notifications.Indexes;

public partial class NotificationIndex : MapIndex
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

public partial class NotificationIndexProvider : IndexProvider<Notification>
{
    public NotificationIndexProvider()
    {
        CollectionName = NotificationConstants.NotificationCollection;
    }

    public override void Describe(DescribeContext<Notification> context)
    {
        context.For<NotificationIndex>()
            .Map(notification =>
            {
                var content = notification.Summary ?? string.Empty;

                var bodyInfo = notification.As<NotificationBodyInfo>();

                if (!string.IsNullOrEmpty(bodyInfo?.TextBody))
                {
                    content += $" {bodyInfo.TextBody}";
                }

                content = StripHTML(content);

                if (content.Length > NotificationConstants.NotificationIndexContentLength)
                {
                    content = content[..NotificationConstants.NotificationIndexContentLength];
                }

                var readInfo = notification.As<NotificationReadInfo>();

                return new NotificationIndex()
                {
                    NotificationId = notification.NotificationId,
                    UserId = notification.UserId,
                    IsRead = readInfo.IsRead,
                    ReadAtUtc = readInfo.ReadAtUtc,
                    CreatedAtUtc = notification.CreatedUtc,
                    Content = content,
                };
            });
    }

    private static readonly Regex _htmlRegex = GetHtmlBlockRegex();

    public static string StripHTML(string html)
    {
        return _htmlRegex.Replace(html, string.Empty);
    }

    [GeneratedRegex("<.*?>", RegexOptions.Compiled)]
    private static partial Regex GetHtmlBlockRegex();
}
