using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Data.Descriptors;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Data
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        ///  Adds services at the host level to load site settings from the file system
        ///  and tenant level services to store states and descriptors in the database.
        /// </summary>
        public static OrchardCoreBuilder AddDataStorage(this OrchardCoreBuilder builder)
        {
            return builder.AddSitesFolder()
                .ConfigureServices((collection, sp) =>
                {
                    AddDescriptorStorageTenantServices(collection);
                });
        }

        /// <summary>
        ///  Adds host level services to load site settings from the file system
        /// </summary>
        public static OrchardCoreBuilder AddSitesFolder(this OrchardCoreBuilder builder)
        {
            AddSitesFolderHostServices(builder.Services);
            return builder;
        }

        /// <summary>
        ///  Adds tenant level services to store states and descriptors in the database.
        /// </summary>
        /// <param name="services"></param>
        public static OrchardCoreBuilder AddDescriptorStorage(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureServices((collection, sp) =>
            {
                AddDescriptorStorageTenantServices(collection);
            });
        }

        public static void AddSitesFolderHostServices(IServiceCollection services)
        {
            services.AddSingleton<IShellSettingsConfigurationProvider, ShellSettingsConfigurationProvider>();
            services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();
            services.AddTransient<IConfigureOptions<ShellOptions>, ShellOptionsSetup>();
        }

        /// <summary>
        /// Per-tenant services to store shell state and shell descriptors in the database.
        /// </summary>
        /// <param name="services"></param>
        public static void AddDescriptorStorageTenantServices(IServiceCollection services)
        {
            services.AddScoped<IShellDescriptorManager, ShellDescriptorManager>();
            services.AddScoped<IShellStateManager, ShellStateManager>();
            services.AddScoped<IShellFeaturesManager, ShellFeaturesManager>();
            services.AddScoped<IShellDescriptorFeaturesManager, ShellDescriptorFeaturesManager>();
        }
    }
}