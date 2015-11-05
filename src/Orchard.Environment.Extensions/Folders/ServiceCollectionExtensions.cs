using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions.Folders.ManifestParsers;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModuleFolder(this IServiceCollection services, string virtualPath, string manifestName = "orchard.json")
        {
            return services.Configure<ExtensionHarvestingOptions>(options =>
            {
                var expander = new ModuleLocationExpander(DefaultExtensionTypes.Module, new[] {virtualPath}, manifestName);
                options.ModuleLocationExpanders.Add(expander);
            });
        }

        public static IServiceCollection AddJsonManifestParser(this IServiceCollection services)
        {
            return services.AddScoped<IManifestParser, JsonManifestParser>();
        }

        public static IServiceCollection AddYamlManifestParser(this IServiceCollection services)
        {
            return services.AddScoped<IManifestParser, YamlManifestParser>();
        }
    }
}