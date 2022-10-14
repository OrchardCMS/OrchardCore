namespace OrchardCore.Notifications.Models;

public class NotificationMessage : INotificationMessage
{
    public string Subject { get; set; }

    public string Body { get; set; }
}
