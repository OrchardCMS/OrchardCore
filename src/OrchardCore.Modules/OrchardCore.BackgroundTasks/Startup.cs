using Microsoft.Extensions.DependencyInjection;
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
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddScoped<BackgroundTaskManager>()
            .AddPermissionProvider<Permissions>()
            .AddNavigationProvider<AdminMenu>()
            .AddScoped<IBackgroundTaskSettingsProvider, BackgroundTaskSettingsProvider>();

        // Builds the rows of the background tasks admin list.
        services.AddDisplayDriver<BackgroundTaskEntry, BackgroundTaskEntryDisplayDriver>();
    }
}
