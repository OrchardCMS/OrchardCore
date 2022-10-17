using System;
using OrchardCore.Notifications;
using YesSql;
using YesSql.Filters.Query.Services;

namespace OrchardCore.Navigation.Core;

public class WebNotificationQueryContext : QueryExecutionContext<WebNotification>
{
    public WebNotificationQueryContext(IServiceProvider serviceProvider, IQuery<WebNotification> query) : base(query)
    {
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }
}
