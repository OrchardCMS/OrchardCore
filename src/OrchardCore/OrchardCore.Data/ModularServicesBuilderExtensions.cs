using OrchardCore.Modules;

namespace OrchardCore.Data
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level data access services.
        /// </summary>
        /// <param name="services"></param>
        public static ModularServicesBuilder AddDataAccess(this ModularServicesBuilder builder)
        {
            builder.Services.ConfigureTenantServices((collection) =>
            {
                collection.AddDataAccess();
            });

            return builder;
        }
    }
}