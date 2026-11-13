using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using YesSql;

namespace OrchardCore.Notifications.Services;

public sealed class NotificationManager : INotificationManager
{
    // The notifications of a filter can be those of many users, so the bulk operations don't keep them all in memory.
    private const int BatchSize = 200;

    private readonly ISession _session;
    private readonly ITagCache _tagCache;
    private readonly IClock _clock;

    public NotificationManager(
        ISession session,
        ITagCache tagCache,
        IClock clock)
    {
        _session = session;
        _tagCache = tagCache;
        _clock = clock;
    }

    public Task<Notification> FindByIdAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(notificationId))
        {
            return Task.FromResult<Notification>(null);
        }

        return _session.Query<Notification, NotificationIndex>(x => x.NotificationId == notificationId, collection: NotificationConstants.NotificationCollection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> ListAsync(NotificationFilter filter, CancellationToken cancellationToken = default)
    {
        IQuery<Notification> query = Query(filter)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id);

        if (filter.Skip > 0)
        {
            query = query.Skip(filter.Skip);
        }

        if (filter.Take.HasValue)
        {
            query = query.Take(filter.Take.Value);
        }

        var notifications = await query.ListAsync(cancellationToken);

        return notifications.ToList();
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        NotificationLengthValidator.Validate(notification.Type, notification.ReferenceId);

        if (!string.IsNullOrEmpty(notification.Url) && !NotificationUrlHelper.IsValid(notification.Url))
        {
            throw new ArgumentException("The notification URL must be an absolute HTTP URL or a local URL.", nameof(notification));
        }

        await _session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection, cancellationToken: cancellationToken);
        await RefreshAsync(notification.UserId);
    }

    public async Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        _session.Delete(notification, collection: NotificationConstants.NotificationCollection);
        await RefreshAsync(notification.UserId);
    }

    public async Task<int> DeleteAsync(NotificationFilter filter, CancellationToken cancellationToken = default)
    {
        var userIds = new HashSet<string>();
        var count = 0;

        while (true)
        {
            var notifications = (await Query(filter).Take(BatchSize).ListAsync(cancellationToken)).ToList();

            if (notifications.Count == 0)
            {
                break;
            }

            foreach (var notification in notifications)
            {
                _session.Delete(notification, collection: NotificationConstants.NotificationCollection);
                userIds.Add(notification.UserId);
                count++;
            }

            await ReleaseAsync(notifications, cancellationToken);
        }

        await RefreshAsync(userIds);

        return count;
    }

    public Task<bool> MarkAsReadAsync(Notification notification, CancellationToken cancellationToken = default)
        => SetReadAsync(notification, true, cancellationToken);

    public Task<bool> MarkAsUnreadAsync(Notification notification, CancellationToken cancellationToken = default)
        => SetReadAsync(notification, false, cancellationToken);

    public async Task<int> MarkAsReadAsync(NotificationFilter filter, CancellationToken cancellationToken = default)
    {
        var userIds = new HashSet<string>();
        var count = 0;

        // The saved notifications leave the query, so only those that didn't change are skipped.
        var skipped = 0;

        while (true)
        {
            var notifications = (await Query(filter)
                .Where(x => !x.IsRead)
                .OrderBy(x => x.Id)
                .Skip(skipped)
                .Take(BatchSize)
                .ListAsync(cancellationToken)).ToList();

            if (notifications.Count == 0)
            {
                break;
            }

            foreach (var notification in notifications)
            {
                if (SetRead(notification, true))
                {
                    await _session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection, cancellationToken: cancellationToken);
                    userIds.Add(notification.UserId);
                    count++;
                }
                else
                {
                    skipped++;
                }
            }

            await ReleaseAsync(notifications, cancellationToken);
        }

        await RefreshAsync(userIds);

        return count;
    }

    public bool IsRead(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return notification.TryGet<NotificationReadInfo>(out var readInfo) && readInfo.IsRead;
    }

    private IQuery<Notification, NotificationIndex> Query(NotificationFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (!filter.HasCriteria)
        {
            throw new ArgumentException("The filter must set at least one of the user id, the type and the reference id.", nameof(filter));
        }

        var query = _session.Query<Notification, NotificationIndex>(collection: NotificationConstants.NotificationCollection);

        if (!string.IsNullOrEmpty(filter.UserId))
        {
            var userId = filter.UserId;
            query = query.Where(x => x.UserId == userId);
        }

        if (!string.IsNullOrEmpty(filter.Type))
        {
            var type = filter.Type;
            query = query.Where(x => x.Type == type);
        }

        if (!string.IsNullOrEmpty(filter.ReferenceId))
        {
            var referenceId = filter.ReferenceId;
            query = query.Where(x => x.ReferenceId == referenceId);
        }

        if (filter.IsRead.HasValue)
        {
            var isRead = filter.IsRead.Value;
            query = query.Where(x => x.IsRead == isRead);
        }

        return query;
    }

    private async Task<bool> SetReadAsync(Notification notification, bool isRead, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (!SetRead(notification, isRead))
        {
            return false;
        }

        await _session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection, cancellationToken: cancellationToken);
        await RefreshAsync(notification.UserId);

        return true;
    }

    private bool SetRead(Notification notification, bool isRead)
    {
        var readInfo = notification.GetOrCreate<NotificationReadInfo>();

        if (readInfo.IsRead == isRead)
        {
            return false;
        }

        readInfo.IsRead = isRead;
        readInfo.ReadAtUtc = isRead ? _clock.UtcNow : null;
        notification.Put(readInfo);

        return true;
    }

    private async Task ReleaseAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken)
    {
        await _session.FlushAsync(cancellationToken);
        _session.Detach(notifications, NotificationConstants.NotificationCollection);
    }

    private async Task RefreshAsync(IEnumerable<string> userIds)
    {
        foreach (var userId in userIds)
        {
            await RefreshAsync(userId);
        }
    }

    private Task RefreshAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Task.CompletedTask;
        }

        return _tagCache.RemoveTagAsync(NotificationsHelper.GetUnreadUserNotificationTagKeyByUserId(userId));
    }
}
