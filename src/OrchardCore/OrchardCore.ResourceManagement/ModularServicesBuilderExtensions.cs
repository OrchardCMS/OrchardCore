using OrchardCore.Modules;

namespace OrchardCore.ResourceManagement
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing resources.
        /// </summary>
        /// <param name="services"></param>
        public static ModularServicesBuilder AddResourceManagement(this ModularServicesBuilder builder)
        {
            builder.Services.ConfigureTenantServices((collection) =>
            {
                collection.AddResourceManagement();
            });

            return builder;
        }
    }
}
