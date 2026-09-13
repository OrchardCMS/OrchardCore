using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment.Remote.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Deployment.Remote;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.FileStorage;
using OrchardCore.Modules;
using OrchardCore.RemoteManagement;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment;

public sealed class Startup : StartupBase
{
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        RemoteDeploymentEndpoints.Map(routes);
        RemoteDeploymentTargetEndpoints.Map(routes);
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient(RemoteDeploymentSender.HttpClientName, client => client.Timeout = TimeSpan.FromMinutes(2))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddScoped<RemoteDeploymentSender>();
        services.TryAddTransient<FileCreationService>();

        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<RemoteInstanceService>();
        services.AddScoped<RemoteClientService>();
        services.AddSingleton<IRemoteManagementCapabilityProvider, RemoteDeploymentCapabilityProvider>();
        services.AddScoped<IDeploymentTargetProvider, RemoteInstanceDeploymentTargetProvider>();
        services.AddPermissionProvider<Permissions>();
    }
}
