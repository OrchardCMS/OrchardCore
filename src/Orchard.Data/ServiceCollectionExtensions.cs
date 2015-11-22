using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IOrchardDataAssemblyProvider, OrchardDataAssemblyProvider>();

            return services;
        }
    }
}