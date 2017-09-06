using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Services;

namespace OrchardCore.Deployment.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeploymentServices(this IServiceCollection services)
        {
            services.AddScoped<IDeploymentManager, DeploymentManager>();

            return services;
        }
    }
}
