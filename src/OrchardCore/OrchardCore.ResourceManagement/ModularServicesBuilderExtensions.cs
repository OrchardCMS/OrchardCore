using OrchardCore.Modules;
using OrchardCore.ResourceManagement.TagHelpers;

namespace OrchardCore.ResourceManagement
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing resources.
        /// </summary>
        public static ModularServicesBuilder AddResourceManagement(this ModularServicesBuilder builder)
        {
            builder.Services
                .AddTagHelpers(typeof(ResourcesTagHelper).Assembly)
                .ConfigureTenantServices((collection) =>
                {
                    collection.AddResourceManagement();
                });

            return builder;
        }
    }
}
