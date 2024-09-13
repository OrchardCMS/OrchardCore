using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment.Remote;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<RemoteInstanceService>();
        services.AddScoped<RemoteClientService>();
        services.AddScoped<IDeploymentTargetProvider, RemoteInstanceDeploymentTargetProvider>();
        services.AddPermissionProvider<Permissions>();
    }
}
