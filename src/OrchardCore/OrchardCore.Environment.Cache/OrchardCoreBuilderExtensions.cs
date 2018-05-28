using OrchardCore.Modules;

namespace OrchardCore.Environment.Cache
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level caching services.
        /// </summary>
        public static OrchardCoreBuilder AddCaching(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices((collection, sp) =>
            {
                collection.AddCaching();
            });

            return builder;
        }
    }
}
