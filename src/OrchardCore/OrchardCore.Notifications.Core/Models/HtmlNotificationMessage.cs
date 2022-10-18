namespace OrchardCore.Notifications.Models;

public class HtmlNotificationMessage : NotificationMessage
{
    public bool BodyContainsHtml { get; set; }
}


public class ContentNotificationMessage : NotificationMessage
{
    public bool BodyContainsHtml { get; set; }

    public string ContentItemId { get; set; }

    public string Url { get; set; }
}
