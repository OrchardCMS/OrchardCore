using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DeploymentServiceCollectionExtensions
{
    public static IServiceCollection AddDeploymentServices(this IServiceCollection services)
    {
        services.AddScoped<IDeploymentManager, DeploymentManager>();

        return services;
    }
}
