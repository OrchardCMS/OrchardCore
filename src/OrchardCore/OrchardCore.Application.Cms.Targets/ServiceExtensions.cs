using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Data;
using OrchardCore.Modules;

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
            services.AddAuthentication();

            services.AddModules()
                .WithDefaultFeatures("OrchardCore.Antiforgery", "OrchardCore.Mvc", "OrchardCore.Settings",
                    "OrchardCore.Setup", "OrchardCore.Recipes", "OrchardCore.Commons");

            return services;
        }
    }
}
