using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks.Services;
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
    }
}
