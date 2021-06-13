using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ResourceManagement;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services for managing resources.
        /// </summary>
        public static IServiceCollection AddResourceManagement(this IServiceCollection services)
        {
            services.TryAddScoped<IResourceManager, ResourceManager>();

            return services;
        }
    }
}
