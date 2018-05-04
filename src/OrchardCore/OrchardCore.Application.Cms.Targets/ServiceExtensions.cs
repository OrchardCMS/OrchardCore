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
                    "OrchardCore.Setup", "OrchardCore.Recipes", "OrchardCore.Commons")

                .ConfigureTenantServices<IOptions<ShellOptions>, ShellSettings>(
                    (collection, options, settings) =>
                {
                    var directory = Directory.CreateDirectory(Path.Combine(
                        options.Value.ShellsApplicationDataPath,
                        options.Value.ShellsContainerName,
                        settings.Name, "DataProtection-Keys"));

                    collection.Add(new ServiceCollection()
                        .AddDataProtection()
                        .PersistKeysToFileSystem(directory)
                        .SetApplicationName(settings.Name)
                        .Services);
                });

            return services;
        }
    }
}
