using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Data.Descriptors;
using OrchardCore.Environment.Shell.Descriptor;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ShellOrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds services at the host level to load site settings from the file system
        /// and tenant level services to store states and descriptors in the database.
        /// </summary>
        public static OrchardCoreBuilder AddDataStorage(this OrchardCoreBuilder builder)
        {
            builder.AddSitesFolder()
                .ConfigureServices(services =>
                {
                    services.AddScoped<IShellDescriptorManager, ShellDescriptorManager>();
                });

            return builder;
        }

        /// <summary>
        /// Host services to load site settings from the file system
        /// </summary>
        public static OrchardCoreBuilder AddSitesFolder(this OrchardCoreBuilder builder)
        {
            var services = builder.ApplicationServices;

            services.AddSingleton<IShellsSettingsSources, ShellsSettingsSources>();
            services.AddSingleton<IShellsConfigurationSources, ShellsConfigurationSources>();
            services.AddSingleton<IShellConfigurationSources, ShellConfigurationSources>();
            services.AddTransient<IConfigureOptions<ShellOptions>, ShellOptionsSetup>();
            services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

            return builder;
        }
    }
}
