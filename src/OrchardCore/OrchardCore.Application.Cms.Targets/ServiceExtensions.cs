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
            services.AddThemingHost();
            services.AddManifestDefinition("theme");
            services.AddSitesFolder();
            services.AddCommands();
            services.AddAntiforgery();
            services.AddAuthentication();
            services.AddModules(modules =>
            {
                modules.WithDefaultFeatures(
                    "OrchardCore.Antiforgery", "OrchardCore.Mvc", "OrchardCore.Settings",
                    "OrchardCore.Setup", "OrchardCore.Recipes", "OrchardCore.Commons");
            });

            return services;
        }
    }
}
