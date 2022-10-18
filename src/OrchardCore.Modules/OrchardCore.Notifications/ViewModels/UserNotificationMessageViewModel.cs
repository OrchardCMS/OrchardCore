using System;

namespace OrchardCore.Notifications.ViewModels;

public class UserNotificationMessageViewModel
{
    public string NotificationId { get; set; }

    public bool IsRead { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public bool IsHtmlBody { get; set; }

    public bool HasBody() => !String.IsNullOrWhiteSpace(Body);
}
