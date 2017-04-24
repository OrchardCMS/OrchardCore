using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell.Data.Descriptors;
using Orchard.Environment.Shell.Descriptor;

namespace Orchard.Environment.Shell.Data
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
            services.Configure<ShellOptions>(options =>
            {
                options.ShellsRootContainerName = rootPath;
                options.ShellsContainerName = sitesPath;
            });

            services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

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