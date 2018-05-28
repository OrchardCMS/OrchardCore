using OrchardCore.Modules;

namespace OrchardCore.Environment.Extensions
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host and tenant level services for managing extensions.
        /// </summary>
        public static OrchardCoreBuilder AddExtensionManager(this OrchardCoreBuilder builder)
        {
            builder.Services.AddExtensionManagerHost();

            builder.Startup.ConfigureServices((collection, sp) =>
            {
                collection.AddExtensionManager();
            });

            return builder;
        }
    }
}