using OrchardCore.Environment.Cache;
using OrchardCore.Users;

namespace OrchardCore.Notifications.Services;

public sealed class CacheNotificationEventsHandler : NotificationEventsHandler
{
    private readonly ITagCache _tagCache;

    public CacheNotificationEventsHandler(ITagCache tagCache)
    {
        _tagCache = tagCache;
    }

    public override Task SentAsync(NotificationContext context)
    {
        if (context.Notify is IUser user)
        {
            return _tagCache.RemoveTagAsync($"{NotificationConstants.TopUnreadUserNotificationCacheTag}:{user.UserName}");
        }

        return Task.CompletedTask;
    }
}
