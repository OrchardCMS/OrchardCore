using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Data.Descriptors;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Data
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds host and tenant level services for tenant shell data.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection WithTenantDataStorage(this IServiceCollection services)
        {
            services.AddSitesFolder();

            return services.ConfigureTenantServices(collection =>
            {
                collection.AddShellDescriptorStorage();
            });
        }

        /// <summary>
        ///  Host services to load site settings from the file system
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sitesPath"></param>
        /// <returns></returns>
        public static IServiceCollection AddSitesFolder(this IServiceCollection services)
        {
            services.AddSingleton<IShellSettingsConfigurationProvider, ShellSettingsConfigurationProvider>();
            services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();
            services.AddTransient<IConfigureOptions<ShellOptions>, ShellOptionsSetup>();

            return services;
        }

        /// <summary>
        /// Per-tenant services to store shell state and shell descriptors in the database.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddShellDescriptorStorage(this IServiceCollection services)
        {
            services.AddScoped<IShellDescriptorManager, ShellDescriptorManager>();
            services.AddScoped<IShellStateManager, ShellStateManager>();
            services.AddScoped<IShellFeaturesManager, ShellFeaturesManager>();
            services.AddScoped<IShellDescriptorFeaturesManager, ShellDescriptorFeaturesManager>();

            return services;
        }
    }
}