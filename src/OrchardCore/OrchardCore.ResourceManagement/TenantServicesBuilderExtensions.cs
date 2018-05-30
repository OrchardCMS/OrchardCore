using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ResourceManagement;
using OrchardCore.ResourceManagement.Filters;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing resources.
        /// </summary>
        public static TenantServicesBuilder AddResourceManagement(this TenantServicesBuilder tenant)
        {
            var services = tenant.Services;

            services.TryAddScoped<IResourceManager, ResourceManager>();
            services.TryAddScoped<IRequireSettingsProvider, DefaultRequireSettingsProvider>();
            services.TryAddSingleton<IResourceManifestState, ResourceManifestState>();

            return tenant;
        }

        /// <summary>
        /// Adds a tenant level mvc filter which registers a Generator Meta Tag.
        /// </summary>
        public static TenantServicesBuilder AddGeneratorTagFilter(this TenantServicesBuilder tenant)
        {
            tenant.Services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MetaGeneratorFilter));
            });

            return tenant;
        }
    }
}