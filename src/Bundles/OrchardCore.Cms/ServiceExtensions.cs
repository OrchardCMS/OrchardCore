using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Commands;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Environment.Shell.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOrchardCms(this IServiceCollection services)
        {
            return AddOrchardCms(services, null);
        }

        public static IServiceCollection AddOrchardCms(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddManifestDefinition("Theme.txt", "theme");
            services.AddSitesFolder("App_Data", "Sites");
            services.AddCommands();
            services.AddModules(modules => 
            {
                if (configuration != null)
                {
                    modules.WithConfiguration(configuration);
                }

                modules.WithDefaultFeatures("Orchard.Mvc", "Orchard.Settings", "Orchard.Setup", "Orchard.Recipes", "Orchard.Commons");
            });

            return services;
        }
    }
}
