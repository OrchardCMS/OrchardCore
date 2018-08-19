using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Deployment
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeploymentTargetHandler<TImplementation>(
            this IServiceCollection serviceCollection)
            where TImplementation : class, IDeploymentTargetHandler
        {
            serviceCollection.AddScoped<IDeploymentTargetHandler, TImplementation>();

            return serviceCollection;
        }
    }
}
