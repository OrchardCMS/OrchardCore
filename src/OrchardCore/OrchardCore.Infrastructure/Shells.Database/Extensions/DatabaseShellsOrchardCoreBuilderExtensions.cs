using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Shells.Database.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseShellsOrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Host services to load shells settings and configuration from database.
        /// </summary>
        public static OrchardCoreBuilder AddDatabaseShellsConfiguration(this OrchardCoreBuilder builder)
        {
            var services = builder.ApplicationServices;

            services.Replace(ServiceDescriptor.Singleton<IShellsSettingsSources, DatabaseShellsSettingsSources>());
            services.Replace(ServiceDescriptor.Singleton<IShellConfigurationSources, DatabaseShellConfigurationSources>());

            return builder;
        }
    }
}
