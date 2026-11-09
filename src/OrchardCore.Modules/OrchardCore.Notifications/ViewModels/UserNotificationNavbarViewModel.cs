namespace OrchardCore.Notifications.ViewModels;

public class UserNotificationNavbarViewModel
{
    public int TotalUnread { get; set; }

    public int MaxVisibleNotifications { get; set; }

    /// <summary>
    /// Gets or sets whether the user may have more unread notifications than <see cref="TotalUnread"/>,
    /// as only the configured maximum of them were evaluated by the visibility handlers.
    /// </summary>
    public bool IsTotalUnreadCapped { get; set; }

    public List<Notification> Notifications { get; set; }
}
