using OrchardCore.Modules;
using OrchardCore.ResourceManagement.TagHelpers;

namespace OrchardCore.ResourceManagement
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing resources.
        /// </summary>
        public static OrchardCoreBuilder AddResourceManagement(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices((collection, sp) =>
            {
                collection.AddResourceManagement();
                collection.AddTagHelpers(typeof(ResourcesTagHelper).Assembly);
            });

            return builder;
        }
    }
}
