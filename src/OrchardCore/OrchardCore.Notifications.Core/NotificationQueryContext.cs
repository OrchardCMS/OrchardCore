using System;
using OrchardCore.Notifications;
using YesSql;
using YesSql.Filters.Query.Services;

namespace OrchardCore.Navigation.Core;

public class NotificationQueryContext : QueryExecutionContext<Notification>
{
    public NotificationQueryContext(IServiceProvider serviceProvider, IQuery<Notification> query) : base(query)
    {
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }
}
