using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Environment.Extensions.Manifests
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddManifestDefinition(
            this IServiceCollection services,
            string definitionName,
            string type)
        {
            return services.Configure<ManifestOptions>(configureOptions: options =>
            {
                var option = new ManifestOption {
                    ManifestFileName = definitionName,
                    Type = type
                };

                options.ManifestConfigurations.Add(option);
            });
        }
    }
}