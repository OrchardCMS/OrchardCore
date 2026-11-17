using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Models;
using YesSql;

namespace OrchardCore.Navigation.Core;

public sealed class DefaultNotificationsAdminListQueryService : INotificationsAdminListQueryService
{
    private readonly YesSql.ISession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationVisibilityEvaluator _visibilityEvaluator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultNotificationsAdminListQueryService(
        YesSql.ISession session,
        IServiceProvider serviceProvider,
        INotificationVisibilityEvaluator visibilityEvaluator,
        IHttpContextAccessor httpContextAccessor)
    {
        _session = session;
        _serviceProvider = serviceProvider;
        _visibilityEvaluator = visibilityEvaluator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<NotificationQueryResult> QueryAsync(int page, int pageSize, ListNotificationOptions options, IUpdateModel updater)
    {
        var query = _session.Query<Notification>(collection: NotificationConstants.NotificationCollection);

        query = await options.FilterResult.ExecuteAsync(new NotificationQueryContext(_serviceProvider, query));

        // Query the count before applying pagination logic.
        var totalCount = await query.CountAsync();

        if (pageSize > 0)
        {
            if (page > 1)
            {
                query = query.Skip((page - 1) * pageSize);
            }

            query = query.Take(pageSize);
        }

        var notifications = (await query.ListAsync()).ToList();

        if (_visibilityEvaluator.HasHandlers)
        {
            // The hidden notifications are removed from the page. Those of the other pages are still counted,
            // as counting them would require evaluating every notification of the user.
            var visible = await _visibilityEvaluator.FilterAsync(notifications, _httpContextAccessor.HttpContext?.User);

            totalCount -= notifications.Count - visible.Count;
            notifications = visible;
        }

        return new NotificationQueryResult()
        {
            Notifications = notifications,
            TotalCount = totalCount,
        };
    }
}
