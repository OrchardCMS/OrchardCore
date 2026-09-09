using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Deployment.Remote;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.Deployment.Remote.Drivers;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.FileStorage;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.TryAddTransient<FileCreationService>();

        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<RemoteInstanceService>();
        services.AddScoped<RemoteClientService>();
        services.AddScoped<IDeploymentTargetProvider, RemoteInstanceDeploymentTargetProvider>();
        services.AddPermissionProvider<Permissions>();

        // Build the rows of the remote instances and remote clients admin lists.
        services.AddDisplayDriver<RemoteInstance, RemoteInstanceDisplayDriver>();
        services.AddDisplayDriver<RemoteClient, RemoteClientDisplayDriver>();
    }
}
