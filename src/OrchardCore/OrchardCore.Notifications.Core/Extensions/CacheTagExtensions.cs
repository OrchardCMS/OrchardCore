using OrchardCore.Notifications;

namespace OrchardCore.Environment.Cache;

public static class CacheTagExtensions
{
    public static Task RemoveUnreadUserNotificationTagAsync(this ITagCache tagCache, string username)
        => tagCache.RemoveTagAsync($"{NotificationConstants.TopUnreadUserNotificationCacheTag}:{username}");
}
