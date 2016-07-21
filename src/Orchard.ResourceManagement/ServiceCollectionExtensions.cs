using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Orchard.ResourceManagement
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResourceManagement(this IServiceCollection services)
        {
            services.TryAddScoped<IResourceManager, ResourceManager>();
            services.TryAddScoped<IRequireSettingsProvider, DefaultRequireSettingsProvider>();
            services.TryAddSingleton<IResourceManifestState, ResourceManifestState>();
            return services;
        }
    }
}
