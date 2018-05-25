using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Extensions.Manifests
{
    public static class ServiceCollectionExtensions
    {
        public static OrchardCoreBuilder AddManifestDefinition(this OrchardCoreBuilder builder, string moduleType)
        {
            builder.Services.Configure<ManifestOptions>(configureOptions: options =>
            {
                var option = new ManifestOption {
                    Type = moduleType
                };

                options.ManifestConfigurations.Add(option);
            });

            return builder;
        }
    }
}