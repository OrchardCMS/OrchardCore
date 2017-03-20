using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Tenant.Data.Descriptors;
using OrchardCore.Tenant.Descriptor;

namespace OrchardCore.Tenant.Data
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///  Host services to load site settings from the file system
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sitesPath"></param>
        /// <returns></returns>
        public static IServiceCollection AddSitesFolder(this IServiceCollection services, string rootPath, string sitesPath)
        {
            services.Configure<TenantOptions>(options =>
            {
                options.TenantsRootContainerName = rootPath;
                options.TenantsContainerName = sitesPath;
            });

            services.AddSingleton<ITenantSettingsManager, TenantSettingsManager>();

            return services;
        }

        /// <summary>
        /// Per-tenant services to store tenant state and tenant descriptors in the database.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTenantDescriptorStorage(this IServiceCollection services)
        {
            services.AddScoped<ITenantDescriptorManager, TenantDescriptorManager>();
            services.AddScoped<ITenantStateManager, TenantStateManager>();
            services.AddScoped<ITenantFeaturesManager, TenantFeaturesManager>();
            services.AddScoped<ITenantDescriptorFeaturesManager, TenantDescriptorFeaturesManager>();

            return services;
        }
    }
}