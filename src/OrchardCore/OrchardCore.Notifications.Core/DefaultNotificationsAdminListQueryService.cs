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

    public async Task<WebNotificationQueryResult> QueryAsync(int page, int pageSize, ListNotificationOptions options, IUpdateModel updater)
    {
        var query = _session.Query<WebNotification>(collection: NotificationConstants.NotificationCollection);

        query = await options.FilterResult.ExecuteAsync(new WebNotificationQueryContext(_serviceProvider, query));

        if (pageSize > 0)
        {
            if (page > 1)
            {
                query = query.Skip((page - 1) * pageSize);
            }

            query = query.Take(pageSize);
        }

        return new WebNotificationQueryResult()
        {
            Notifications = await query.ListAsync(),
            TotalCount = await query.CountAsync(),
        };
    }
}
