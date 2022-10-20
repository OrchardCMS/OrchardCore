namespace OrchardCore.Notifications.Models;

public class HtmlNotificationMessage : NotificationMessage, INotificationBodyMessage
{
    public string Body { get; set; }

    public bool IsHtmlBody { get; set; }
}

public class ContentNotificationMessage : NotificationMessage, INotificationBodyMessage, INotificationContentMessage
{
    public string Body { get; set; }

    public bool IsHtmlBody { get; set; }

    public NotificationLinkType LinkType { get; set; }

    public string ContentItemId { get; set; }

    public string CustomUrl { get; set; }

    public string ContentType { get; set; }

    public string ContentOwnerId { get; set; }
}
