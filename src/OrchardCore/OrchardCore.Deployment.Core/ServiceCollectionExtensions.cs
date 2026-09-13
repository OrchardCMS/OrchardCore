using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Services;

namespace OrchardCore.Deployment.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeploymentServices(this IServiceCollection services)
    {
        services.AddScoped<DeploymentExecutionContext>();
        services.AddScoped<IDeploymentManager, DeploymentManager>();
        services.AddScoped<IDeploymentArchiveService, DeploymentArchiveService>();
        services.AddOptions<DeploymentPackageOptions>();
        services.AddScoped<DeploymentPackageService>();

        return services;
    }
}
