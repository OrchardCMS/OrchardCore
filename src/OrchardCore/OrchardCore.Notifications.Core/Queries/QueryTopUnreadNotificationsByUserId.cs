using System.Linq.Expressions;
using OrchardCore.Notifications.Indexes;
using YesSql;

namespace OrchardCore.Notifications.Queries;

public class QueryTopUnreadNotificationsByUserId : ICompiledQuery<Notification>
{
    private readonly string _userId;
    private readonly int _total;

    public QueryTopUnreadNotificationsByUserId(string userId, int total)
    {
        _userId = userId;
        _total = total;
    }

    public Expression<Func<IQuery<Notification>, IQuery<Notification>>> Query()
    {
        return query => query.With<NotificationIndex>(x => x.UserId == _userId && x.IsRead == false)
        .OrderByDescending(x => x.CreatedAtUtc)
        .Take(_total);
    }
}
