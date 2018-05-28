using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Liquid
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing liquid view template files.
        /// </summary>
        public static OrchardCoreBuilder AddLiquidViews(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices((collection, sp) =>
            {
                collection.AddLiquidViews();
            });

            return builder;
        }
    }
}