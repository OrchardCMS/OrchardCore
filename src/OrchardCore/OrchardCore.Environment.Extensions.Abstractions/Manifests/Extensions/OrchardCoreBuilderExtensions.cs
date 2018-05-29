using OrchardCore.Environment.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Host level options configuration to add a new module type.
        /// </summary>
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