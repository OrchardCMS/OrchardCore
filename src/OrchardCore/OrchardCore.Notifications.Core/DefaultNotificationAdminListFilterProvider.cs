using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Navigation.Core;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.Notifications;

public class DefaultNotificationAdminListFilterProvider : INotificationAdminListFilterProvider
{
    public void Build(QueryEngineBuilder<WebNotification> builder)
    {
        builder
            .WithNamedTerm("status", builder => builder
                .OneCondition((val, query, ctx) =>
                {
                    if (Enum.TryParse<NotificationStatus>(val, true, out var status))
                    {
                        switch (status)
                        {
                            case NotificationStatus.Read:
                                query.With<WebNotificationIndex>(x => x.IsRead);
                                break;
                            case NotificationStatus.Unread:
                                query.With<WebNotificationIndex>(x => !x.IsRead);
                                break;
                            default:
                                break;
                        }
                    }

                    return new ValueTask<IQuery<WebNotification>>(query);
                })
                .MapTo<ListNotificationOptions>((val, model) =>
                {
                    if (Enum.TryParse<NotificationStatus>(val, true, out var status))
                    {
                        model.Status = status;
                    }
                })
                .MapFrom<ListNotificationOptions>((model) =>
                {
                    if (model.Status.HasValue)
                    {
                        return (true, model.Status.ToString());
                    }

                    return (false, String.Empty);
                })
                .AlwaysRun()
             )
            .WithNamedTerm("sort", builder => builder
                .OneCondition((val, query, ctx) =>
                {
                    if (Enum.TryParse<NotificationOrder>(val, true, out var sort) && sort == NotificationOrder.Oldest)
                    {
                        return new ValueTask<IQuery<WebNotification>>(query.With<WebNotificationIndex>().OrderBy(x => x.CreatedAtUtc));
                    }

                    return new ValueTask<IQuery<WebNotification>>(query.With<WebNotificationIndex>().OrderByDescending(x => x.CreatedAtUtc));
                })
                .MapTo<ListNotificationOptions>((val, model) =>
                {
                    if (Enum.TryParse<NotificationOrder>(val, true, out var sort))
                    {
                        model.OrderBy = sort;
                    }
                })
                .MapFrom<ListNotificationOptions>((model) =>
                {
                    if (model.OrderBy.HasValue)
                    {
                        return (true, model.OrderBy.ToString());
                    }

                    return (false, String.Empty);
                })
                .AlwaysRun()
            )
            .WithDefaultTerm("owner", builder => builder
                .OneCondition((val, query, ctx) =>
                {
                    var context = (WebNotificationQueryContext)ctx;
                    var httpAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

                    var userId = httpAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    return new ValueTask<IQuery<WebNotification>>(query.With<WebNotificationIndex>(t => t.UserId == userId));
                })
                .AlwaysRun()
            )
            .WithDefaultTerm("text", builder => builder
                .ManyCondition(
                        (val, query) => query.With<WebNotificationIndex>(x => x.Content.Contains(val)),
                        (val, query) => query.With<WebNotificationIndex>(x => x.Content.NotContains(val))
                )
            );
    }
}
