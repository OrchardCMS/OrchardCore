namespace OrchardCore.Notifications.Models;

public class NotificationBodyInfo
{
    /// <summary>
    /// The Summary or subject of the notification.
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// The text body of the notification.
    /// </summary>
    public string TextBody { get; set; }

    /// <summary>
    /// The html body of the notification.
    /// </summary>
    public string HtmlBody { get; set; }
}
