using System;
using System.Collections.Generic;

namespace OrchardCore.Notifications.ViewModels;

public class UserWebNotificationViewModel
{
    public int TotalUnread { get; set; }

    public int MaxVisibleNotifications { get; set; }

    public List<UserWebNotificationMessageViewModel> Notifications { get; set; }
}

public class UserWebNotificationMessageViewModel
{
    public string NotificationId { get; set; }

    public bool IsRead { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public bool IsHtmlBody { get; set; }

    public bool HasBody() => !String.IsNullOrWhiteSpace(Body);
}
