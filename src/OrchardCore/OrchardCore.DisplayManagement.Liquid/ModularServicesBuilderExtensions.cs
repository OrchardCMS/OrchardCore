using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Liquid
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing liquid view template files.
        /// </summary>
        /// <param name="services"></param>
        public static ModularServicesBuilder AddLiquidViews(this ModularServicesBuilder builder)
        {
            builder.Services.ConfigureTenantServices((collection) =>
            {
                collection.AddLiquidViews();
            });

            return builder;
        }
    }
}