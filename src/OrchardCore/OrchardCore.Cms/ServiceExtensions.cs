using Microsoft.Extensions.Configuration;
using Orchard.Environment.Commands;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Environment.Shell.Data;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOrchardCms(this IServiceCollection services, params string[] extraDefaultFeatureIds)
        {
            return AddOrchardCms(services, null, extraDefaultFeatureIds);
        }

        public static IServiceCollection AddOrchardCms(this IServiceCollection services, IConfiguration configuration, params string[] extraDefaultFeatureIds)
        {
            services.AddManifestDefinition("Theme.txt", "theme");
            services.AddExtensionLocation("Themes");
            services.AddSitesFolder("App_Data", "Sites");
            services.AddCommands();
            services.AddAuthentication();
            services.AddModules(modules => 
            {
                if (configuration != null)
                {
                    modules.WithConfiguration(configuration);
                }

                var defultFeatureIds = new string[] { "Orchard.Mvc", "Orchard.Settings", "Orchard.Setup", "Orchard.Recipes", "Orchard.Commons" };
                modules.WithDefaultFeatures(defultFeatureIds.Union(extraDefaultFeatureIds).ToArray());
            });

            return services;
        }
    }
}
