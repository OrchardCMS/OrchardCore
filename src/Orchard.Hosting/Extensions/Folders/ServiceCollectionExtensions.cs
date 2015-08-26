using Microsoft.Framework.DependencyInjection;
using Orchard.Hosting.Extensions.Models;

namespace Orchard.Hosting.Extensions.Folders {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddModuleFolder(
            [NotNull] this IServiceCollection services,
            [NotNull] string virtualPath) {
            return services.Configure<ExtensionHarvestingOptions>(options => {
                var expander = new ModuleLocationExpander(
                    DefaultExtensionTypes.Module,
                    new[] { virtualPath },
                    "Module.txt"
                    );

                options.ModuleLocationExpanders.Add(expander);
            });
        }
    }
}