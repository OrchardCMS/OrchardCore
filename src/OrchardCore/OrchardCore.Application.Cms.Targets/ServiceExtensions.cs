using Microsoft.Extensions.Configuration;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell.Data;

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
            services.AddThemingHost();
            services.AddManifestDefinition("Theme.txt", "theme");
            services.AddSitesFolder();
            services.AddCommands();
            services.AddAuthentication();
            services.AddModules(modules => 
            {
                if (configuration != null)
                {
                    modules.WithConfiguration(configuration);
                }

                modules.WithDefaultFeatures("OrchardCore.Mvc", "OrchardCore.Settings", "OrchardCore.Setup", "OrchardCore.Recipes", "OrchardCore.Commons");
            });

            return services;
        }
    }
}
