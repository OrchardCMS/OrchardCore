using YesSql.Filters.Query;

namespace OrchardCore.Notifications;

public interface INotificationAdminListFilterProvider
{
    void Build(QueryEngineBuilder<Notification> builder);
}
