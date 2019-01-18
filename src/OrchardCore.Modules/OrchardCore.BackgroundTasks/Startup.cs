using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddScoped<BackgroundTaskManager>()
                .AddScoped<IPermissionProvider, Permissions>()
                .AddScoped<INavigationProvider, AdminMenu>()
                .AddScoped<IBackgroundTaskSettingsProvider, BackgroundTaskSettingsProvider>();
        }
    }
}
