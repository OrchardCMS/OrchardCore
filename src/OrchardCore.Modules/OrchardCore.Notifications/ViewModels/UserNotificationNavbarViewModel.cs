namespace OrchardCore.Notifications.ViewModels;

public class UserNotificationNavbarViewModel
{
    public int TotalUnread { get; set; }

    public int MaxVisibleNotifications { get; set; }

    public List<Notification> Notifications { get; set; }
}
