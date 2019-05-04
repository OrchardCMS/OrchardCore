using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IFileVersionProvider));
            if (serviceDescriptor != null)
            {
                services.Remove(serviceDescriptor);
            }
            services.TryAddScoped<IFileVersionProvider, FileVersionProvider>();
            services.TryAddScoped<IResourceManager, ResourceManager>();
            services.TryAddSingleton<IResourceManifestState, ResourceManifestState>();

            return services;
        }
    }
}
