using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement.TagHelpers;

namespace OrchardCore.ResourceManagement
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host and tenant level services for managing resources.
        /// </summary>
        public static OrchardCoreBuilder AddResourceManagement(this OrchardCoreBuilder builder)
        {
            builder.Services.AddTagHelpers(typeof(ResourcesTagHelper).Assembly);

            return builder.ConfigureTenantServices((collection) =>
            {
                AddResourceManagementTenantServices(collection);
            });
        }

        public static void AddResourceManagementTenantServices(IServiceCollection services)
        {
            services.TryAddScoped<IResourceManager, ResourceManager>();
            services.TryAddScoped<IRequireSettingsProvider, DefaultRequireSettingsProvider>();
            services.TryAddSingleton<IResourceManifestState, ResourceManifestState>();
        }
    }
}
