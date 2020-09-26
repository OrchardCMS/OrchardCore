using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Shells.Database.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseShellsOrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Registers host services to load shells settings and configuration from the database,
        /// but only if the related options are provided by the application level configuration.
        /// </summary>
        public static OrchardCoreBuilder AddDatabaseShellsConfiguration(this OrchardCoreBuilder builder)
        {
            var options = builder.Configuration
                .GetSection("OrchardCore")
                .GetSectionCompat("OrchardCore_Shells_Database")
                .Get<DatabaseShellsStorageOptions>();

            if (options != null)
            {
                var services = builder.ApplicationServices;

                services.Replace(ServiceDescriptor.Singleton<IShellsSettingsSources, DatabaseShellsSettingsSources>());
                services.Replace(ServiceDescriptor.Singleton<IShellConfigurationSources, DatabaseShellConfigurationSources>());
            }

            return builder;
        }
    }
}
