using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.BackgroundTasks.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.BackgroundTasks.Drivers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks;

public sealed class Startup : StartupBase
{
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddBackgroundTaskEndpoints();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRemoteManagementCapabilityProvider, BackgroundTasksCapabilityProvider>();
        services.AddScoped<BackgroundTaskManagementService>();
        services
            .AddScoped<BackgroundTaskManager>()
            .AddPermissionProvider<Permissions>()
            .AddNavigationProvider<AdminMenu>()
            .AddScoped<IBackgroundTaskSettingsProvider, BackgroundTaskSettingsProvider>();

        // Builds the rows of the background tasks admin list.
        services.AddDisplayDriver<BackgroundTaskEntry, BackgroundTaskEntryDisplayDriver>();

    }
}
