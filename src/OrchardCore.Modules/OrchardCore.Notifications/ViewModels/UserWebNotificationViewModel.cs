using System.Collections.Generic;

namespace OrchardCore.Notifications.ViewModels;

public class UserWebNotificationViewModel
{
    public int TotalUnread { get; set; }

    public int MaxVisibleNotifications { get; set; }

    public List<UserWebNotificationMessageViewModel> Notifications { get; set; }
}
