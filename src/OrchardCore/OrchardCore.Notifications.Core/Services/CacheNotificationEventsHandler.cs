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

    /// <summary>
    /// Clears the unread notification cache for the notified user after a notification is sent.
    /// </summary>
    /// <param name="context">The notification context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the cache invalidation finishes.</returns>
    public override Task SentAsync(NotificationContext context, CancellationToken cancellationToken = default)
    {
        if (context.Notify is IUser user)
        {
            return _tagCache.RemoveTagAsync($"{NotificationConstants.TopUnreadUserNotificationCacheTag}:{user.UserName}");
        }

        return Task.CompletedTask;
    }
}
