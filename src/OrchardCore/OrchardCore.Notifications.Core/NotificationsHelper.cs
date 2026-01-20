namespace OrchardCore.Notifications;

public static class NotificationsHelper
{
    public static string GetUnreadUserNotificationTagKey(string username)
        => $"{NotificationConstants.TopUnreadUserNotificationCacheTag}:{username}";
}
