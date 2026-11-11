namespace OrchardCore.Notifications;

public static class NotificationsHelper
{
    public static string GetUnreadUserNotificationTagKey(string username)
        => $"{NotificationConstants.TopUnreadUserNotificationCacheTag}:{username}";

    /// <summary>
    /// Gets the cache tag of the unread notifications of a user, by user id.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>The cache tag.</returns>
    public static string GetUnreadUserNotificationTagKeyByUserId(string userId)
        => $"{NotificationConstants.TopUnreadUserNotificationCacheTag}:id:{userId}";
}
