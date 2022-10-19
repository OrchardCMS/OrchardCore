using OrchardCore.Users;

namespace OrchardCore.Notifications;

public class NotificationContext
{
    public Notification Notification { get; set; }

    public INotificationMessage NotificationMessage { get; set; }

    public IUser User { get; set; }
}
