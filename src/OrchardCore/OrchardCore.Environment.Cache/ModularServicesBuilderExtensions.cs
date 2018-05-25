using OrchardCore.Modules;

namespace OrchardCore.Environment.Cache
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level caching services.
        /// </summary>
        public static OrchardCoreBuilder AddCaching(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureTenantServices((collection) =>
            {
                collection.AddCaching();
            });
        }
    }
}
