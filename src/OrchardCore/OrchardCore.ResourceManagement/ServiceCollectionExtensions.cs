using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Modules;

namespace OrchardCore.ResourceManagement
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing resources.
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection WithResourceManagement(this IServiceCollection services)
        {
            return services.ConfigureTenantServices((collection) =>
            {
                collection.AddResourceManagement();
            });
        }

        public static IServiceCollection AddResourceManagement(this IServiceCollection services)
        {
            services.TryAddScoped<IResourceManager, ResourceManager>();
            services.TryAddScoped<IRequireSettingsProvider, DefaultRequireSettingsProvider>();
            services.TryAddSingleton<IResourceManifestState, ResourceManifestState>();
            return services;
        }
    }
}
