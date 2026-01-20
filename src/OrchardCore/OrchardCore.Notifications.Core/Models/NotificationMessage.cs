namespace OrchardCore.Notifications.Models;

public class NotificationMessage : INotificationMessage
{
    public string Subject { get; set; }

    public string Summary { get; set; }

    public string TextBody { get; set; }

    public string HtmlBody { get; set; }

    public bool IsHtmlPreferred { get; set; }
}
