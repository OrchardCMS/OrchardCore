using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Liquid
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing liquid view template files.
        /// </summary>
        public static OrchardCoreBuilder AddLiquidViews(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureTenantServices((collection) =>
            {
                collection.AddLiquidViews();
            });
        }
    }
}