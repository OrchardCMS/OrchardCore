using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Environment.Extensions.Manifests
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddManifestDefinition(
            this IServiceCollection services,
            string definitionName)
        {
            return services.Configure<ManifestOptions>(configureOptions: options =>
            {
                var option = new ManifestOption { ManifestFileName = definitionName };

                options.ManifestConfigurations.Add(option);
            });
        }
    }
}