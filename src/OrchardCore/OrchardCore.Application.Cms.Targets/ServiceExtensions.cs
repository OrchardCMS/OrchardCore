using System;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell.Data;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddOrchardCms(this IServiceCollection services,
            Action<ModularServiceCollection> configure = null)
        {
            services.AddThemingHost();
            services.AddManifestDefinition("theme");
            services.AddSitesFolder();
            services.AddCommands();
            services.AddAntiforgery();
            services.AddAuthentication();
            services.AddModules(modules => 
            {
                configure?.Invoke(modules);

                modules.WithDefaultFeatures(
                    "OrchardCore.Antiforgery",
					"OrchardCore.Mvc", 
                    "OrchardCore.Settings", 
                    "OrchardCore.Setup",
                    "OrchardCore.Recipes", 
                    "OrchardCore.Commons", 
                    "OrchardCore.Apis.GraphQL",
                    "OrchardCore.Apis.JsonApi",
                    "OrchardCore.Apis.OpenApi");
            });

            return services;
        }
    }
}
