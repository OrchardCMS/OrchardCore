using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Abstractions.Environment;
using OrchardVNext.FileSystem;
using OrchardVNext.Hosting.Extensions.Folders;
using OrchardVNext.Hosting.Extensions.Models;

namespace OrchardVNext.Hosting {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddWebHost([NotNull] this IServiceCollection services) {
            return services.AddHost(internalServices => {
                internalServices.AddLogging();

                internalServices.AddHostCore();

                internalServices.Configure<ExtensionHarvestingOptions>(options => {
                    var expander = new ModuleLocationExpander(
                        DefaultExtensionTypes.Module,
                        new[] { "~/Core/OrchardVNext.Core", "~/Modules" },
                        "Module.txt"
                        );

                    options.ModuleLocationExpanders.Add(expander);
                });

                internalServices.AddWebFileSystems();
                
                internalServices.AddSingleton<IHostEnvironment, WebHostEnvironment>();
            });
        }
    }
}