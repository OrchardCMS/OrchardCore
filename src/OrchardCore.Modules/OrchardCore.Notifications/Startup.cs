using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation.Core;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.Drivers;
using OrchardCore.Notifications.Endpoints.Management;
using OrchardCore.Notifications.Handlers;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Migrations;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Services;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Helpers;
using YesSql.Filters.Query;

namespace OrchardCore.Notifications;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationMethodProviderAccessor, NotificationMethodProviderAccessor>();

        services.AddDataMigration<NotificationMigrations>();
        services.AddIndexProvider<NotificationIndexProvider>();
        services.AddScoped<INotificationsAdminListQueryService, DefaultNotificationsAdminListQueryService>();
        services.Configure<StoreCollectionOptions>(o => o.Collections.Add(NotificationConstants.NotificationCollection));
        services.AddScoped<INotificationEvents, CoreNotificationEventsHandler>();

        services.AddPermissionProvider<NotificationPermissionsProvider>();
        services.AddDisplayDriver<ListNotificationOptions, ListNotificationOptionsDisplayDriver>();
        services.AddDisplayDriver<Notification, NotificationDisplayDriver>();
        services.AddTransient<INotificationAdminListFilterProvider, DefaultNotificationsAdminListFilterProvider>();
        services.AddSingleton<INotificationAdminListFilterParser>(sp =>
        {
            var filterProviders = sp.GetServices<INotificationAdminListFilterProvider>();
            var builder = new QueryEngineBuilder<Notification>();
            foreach (var provider in filterProviders)
            {
                provider.Build(builder);
            }

            var parser = builder.Build();

            return new DefaultNotificationAdminListFilterParser(parser);
        });

        services.Configure<NotificationOptions>(_shellConfiguration.GetSection("OrchardCore_Notifications"));

        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, NotificationOptionsConfiguration>();
        services.AddDisplayDriver<User, UserNotificationPreferencesPartDisplayDriver>();
        services.AddDisplayDriver<Navbar, NotificationNavbarDisplayDriver>();
        services.AddScoped<INotificationEvents, CacheNotificationEventsHandler>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddMarkAsReadEndpoint();
    }
}

[RequireFeatures("OrchardCore.Workflows")]
public sealed class WorkflowsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<NotifyUserTask, NotifyUserTaskDisplayDriver>();
    }
}

[RequireFeatures("OrchardCore.Workflows", UserConstants.Features.Users, "OrchardCore.Contents")]
public sealed class UsersWorkflowStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<NotifyContentOwnerTask, NotifyContentOwnerTaskDisplayDriver>();
    }
}

[Feature("OrchardCore.Notifications.Email")]
public sealed class EmailNotificationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationMethodProvider, EmailNotificationProvider>();
    }
}
