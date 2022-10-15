using System.Collections.Generic;

namespace OrchardCore.Notifications.ViewModels;

public class UserWebNotificationViewModel
{
    public int TotalUnread { get; set; }

    public IEnumerable<UserWebNotificationMessageViewModel> Notifications { get; set; }
}

public class UserWebNotificationMessageViewModel
{
    public string MessageId { get; set; }

    public bool IsRead { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }
}
