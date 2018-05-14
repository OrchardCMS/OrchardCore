using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Extensions.Manifests
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddManifestDefinition(
            this IServiceCollection services,
            string moduleType)
        {
            return services.Configure<ManifestOptions>(configureOptions: options =>
            {
                var option = new ManifestOption {
                    Type = moduleType
                };

                options.ManifestConfigurations.Add(option);
            });
        }
    }
}