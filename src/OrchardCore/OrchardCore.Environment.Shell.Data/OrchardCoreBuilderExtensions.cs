using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        ///  Adds services at the host level to load site settings from the file system
        ///  and tenant level services to store states and descriptors in the database.
        /// </summary>
        public static OrchardCoreBuilder AddDataStorage(this OrchardCoreBuilder builder)
        {
            builder.AddSitesFolder()
                .Startup.ConfigureServices((tenant, sp) =>
                {
                    tenant.AddShellDescriptorStorage();
                });

            return builder;
        }

        /// <summary>
        ///  Host services to load site settings from the file system
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sitesPath"></param>
        /// <returns></returns>
        public static OrchardCoreBuilder AddSitesFolder(this OrchardCoreBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IShellSettingsConfigurationProvider, ShellSettingsConfigurationProvider>();
            services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();
            services.AddTransient<IConfigureOptions<ShellOptions>, ShellOptionsSetup>();

            return builder;
        }
    }
}