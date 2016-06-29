using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModuleFolder(
            this IServiceCollection services,
            string virtualPath)
        {
            return services.Configure<ExtensionHarvestingOptions>(configureOptions: options =>
            {
                var expander = new ExtensionLocationExpander(
                    DefaultExtensionTypes.Module,
                    new[] { virtualPath },
                    "Module.txt"
                    );

                options.ExtensionLocationExpanders.Add(expander);
            });
        }

        public static IServiceCollection AddThemeFolder(
            this IServiceCollection services,
            string virtualPath)
        {
            return services.Configure<ExtensionHarvestingOptions>(configureOptions: options =>
             {
                 var expander = new ExtensionLocationExpander(
                     DefaultExtensionTypes.Theme,
                     new[] { virtualPath },
                     "Theme.txt"
                     );

                 options.ExtensionLocationExpanders.Add(expander);
             });
        }
    }
}