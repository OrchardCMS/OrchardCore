using System;
using Microsoft.Extensions.Configuration;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell.Data;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOrchardCms(this IServiceCollection services, 
            IConfiguration configuration = null, 
            Action<ModularServiceCollection> configure = null)
        {
            services.AddThemingHost();
            services.AddManifestDefinition("Theme.txt", "theme");
            services.AddExtensionLocation("Themes");
            services.AddSitesFolder();
            services.AddCommands();
            services.AddAuthentication();
            services.AddModules(modules =>
            {
                if (configuration != null)
                {
                    modules.WithConfiguration(configuration);
                }

                configure?.Invoke(modules);

                modules.WithDefaultFeatures(
                    "OrchardCore.Mvc",
                    "OrchardCore.Settings",
                    "OrchardCore.Setup",
                    "OrchardCore.Recipes",
                    "OrchardCore.Apis.GraphQL",
                    "OrchardCore.Apis.JsonApi",
                    "OrchardCore.Commons");
            });

            return services;
        }
    }
}
