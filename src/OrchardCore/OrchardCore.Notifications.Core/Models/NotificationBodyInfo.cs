namespace OrchardCore.Notifications.Models;

public class NotificationBodyInfo
{
    /// <summary>
    /// The body of the notification.
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Whether or not the body is an HTML.
    /// </summary>
    public bool IsHtmlBody { get; set; }
}
