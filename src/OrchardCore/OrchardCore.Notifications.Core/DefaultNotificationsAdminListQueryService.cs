using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Models;
using YesSql;

namespace OrchardCore.Navigation.Core;

public class DefaultNotificationsAdminListQueryService : INotificationsAdminListQueryService
{
    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;

    public DefaultNotificationsAdminListQueryService(
        ISession session,
        IServiceProvider serviceProvider)
    {
        _session = session;
        _serviceProvider = serviceProvider;
    }

    public async Task<IQuery<WebNotification>> QueryAsync(ListNotificationOptions options, IUpdateModel updater)
    {
        var query = _session.Query<WebNotification>(collection: WebNotification.Collection);

        query = await options.FilterResult.ExecuteAsync(new WebNotificationQueryContext(_serviceProvider, query));

        return query;
    }
}
