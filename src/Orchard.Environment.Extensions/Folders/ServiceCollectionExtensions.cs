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
            return services.Configure<ExtensionHarvestingOptions>(options =>
            {
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