using Microsoft.Extensions.DependencyInjection;
using Orchard.Deployment.Core.Services;
using Orchard.Deployment.Services;

namespace Orchard.Deployment.Core
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
