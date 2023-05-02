using System.Collections.Generic;

namespace OrchardCore.Notifications.ViewModels;

public class UserNotificationCollectionViewModel
{
    public int TotalUnread { get; set; }

    public int MaxVisibleNotifications { get; set; }

    public List<dynamic> Notifications { get; set; }
}
