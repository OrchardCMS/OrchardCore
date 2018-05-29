using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ResourceManagement;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantServicesBuilderExtensions
    {
        public static TenantServicesBuilder AddResourceManagement(this TenantServicesBuilder tenant)
        {
            var services = tenant.Services;

            services.TryAddScoped<IResourceManager, ResourceManager>();
            services.TryAddScoped<IRequireSettingsProvider, DefaultRequireSettingsProvider>();
            services.TryAddSingleton<IResourceManifestState, ResourceManifestState>();

            return tenant;
        }
    }
}