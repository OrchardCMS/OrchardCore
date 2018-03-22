using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddScoped<Services.BackgroundTaskManager>()
                .AddScoped<IPermissionProvider, Permissions>()
                .AddScoped<INavigationProvider, AdminMenu>()
                .AddScoped<IBackgroundTaskDefinitionProvider, Services.BackgroundTaskDefinitionProvider>();
        }
    }
}
