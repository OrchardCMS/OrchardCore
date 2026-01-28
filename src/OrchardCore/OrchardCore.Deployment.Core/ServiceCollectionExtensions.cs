using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Services;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Deployment.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeploymentServices(this IServiceCollection services)
    {
        services.AddScoped<IDeploymentManager, DeploymentManager>();
        services.AddScoped<IFeatureUsageChecker, DeploymentFeatureUsageChecker>();

        return services;
    }
}
