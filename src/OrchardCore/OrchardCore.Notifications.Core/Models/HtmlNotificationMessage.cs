namespace OrchardCore.Notifications.Models;

public class HtmlNotificationMessage : NotificationMessage, INotificationBodyMessage
{
    public string Body { get; set; }

    public bool IsHtmlBody { get; set; }
}
