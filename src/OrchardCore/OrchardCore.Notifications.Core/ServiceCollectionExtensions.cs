using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Navigation.Core;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Migrations;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Services;
using YesSql.Indexes;

namespace OrchardCore.Notifications;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebNotifications(this IServiceCollection services)
    {
        services.TryAddScoped<INotificationMethodProvider, WebNotificationProvider>();
        services.AddContentPart<WebNotificationPart>();
        services.AddDataMigration<NotificationMigrations>();
        services.TryAddSingleton<IIndexProvider, WebNotificationIndexProvider>();
        services.TryAddScoped<INotificationsAdminListQueryService, DefaultNotificationsAdminListQueryService>();
        services.Configure<StoreCollectionOptions>(o => o.Collections.Add(NotificationConstants.NotificationCollection));

        return services;
    }
}
