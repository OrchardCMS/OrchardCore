using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation.Core;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.Controllers;
using OrchardCore.Notifications.Drivers;
using OrchardCore.Notifications.Filters;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Migrations;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Services;
using OrchardCore.Security.Permissions;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Helpers;
using YesSql.Filters.Query;
using YesSql.Indexes;

namespace OrchardCore.Notifications;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationManager, NotificationManager>();
        services.AddScoped<IDataMigration, UserMigrations>();
        services.AddScoped<IDisplayDriver<User>, UserNotificationPartDisplayDriver>();
    }
}

[Feature("OrchardCore.Notifications")]
[RequireFeatures("OrchardCore.Workflows")]
public class WorkflowsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<NotifyUserTask, NotifyUserTaskDisplayDriver>();
    }
}

[Feature("OrchardCore.Notifications")]
[RequireFeatures("OrchardCore.Workflows", "OrchardCore.Users")]
public class UsersStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<NotifyContentOwnerTask, NotifyContentOwnerTaskDisplayDriver>();
    }
}

[Feature("OrchardCore.Notifications.Web")]
public class WebNotificationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationMethodProvider, WebNotificationProvider>();
        services.AddContentPart<WebNotificationPart>();
        services.AddDataMigration<NotificationMigrations>();
        services.AddSingleton<IIndexProvider, WebNotificationIndexProvider>();
        services.AddScoped<IPermissionProvider, WebNotificationPermissionsProvider>();
        services.AddScoped<IDisplayDriver<ListNotificationOptions>, NotificationOptionsDisplayDriver>();
        services.AddScoped<INotificationsAdminListQueryService, DefaultNotificationsAdminListQueryService>();

        services.AddTransient<INotificationAdminListFilterProvider, DefaultNotificationAdminListFilterProvider>();
        services.Configure<StoreCollectionOptions>(o => o.Collections.Add(WebNotification.Collection));

        services.AddSingleton<INotificationAdminListFilterParser>(sp =>
        {
            var filterProviders = sp.GetServices<INotificationAdminListFilterProvider>();
            var builder = new QueryEngineBuilder<WebNotification>();
            foreach (var provider in filterProviders)
            {
                provider.Build(builder);
            }

            var parser = builder.Build();

            return new DefaultNotificationAdminListFilterParser(parser);
        });

        services.Configure<MvcOptions>((options) =>
        {
            options.Filters.Add(typeof(WebNotificationResultFilter));
        });
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var adminControllerName = typeof(AdminController).ControllerName();

        routes.MapAreaControllerRoute(
            name: "ListWebNotifications",
            areaName: "OrchardCore.Notifications",
            pattern: "notifications",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.List) }
        );

        routes.MapAreaControllerRoute(
            name: "ReadAllWebNotifications",
            areaName: "OrchardCore.Notifications",
            pattern: "notifications/read-all",
            defaults: new { controller = adminControllerName, action = nameof(AdminController.ReadAll) }
        );
    }
}

[Feature("OrchardCore.Notifications.Email")]
public class EmailNotificationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationMethodProvider, EmailNotificationProvider>();
    }
}
