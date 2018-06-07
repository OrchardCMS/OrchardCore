using OrchardCore.ResourceManagement.TagHelpers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing resources.
        /// </summary>
        public static OrchardCoreBuilder AddResourceManagement(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices(tenant =>
            {
                tenant.AddResourceManagement();
                tenant.Services.AddTagHelpers(typeof(ResourcesTagHelper).Assembly);
            });

            return builder;
        }

        /// <summary>
        /// Adds a tenant level mvc filter which registers a Generator Meta Tag.
        /// </summary>
        public static OrchardCoreBuilder AddGeneratorTagFilter(this OrchardCoreBuilder builder)
        {
            builder.Startup.ConfigureServices(tenant =>
            {
                tenant.AddGeneratorTagFilter();
            });

            return builder;
        }
    }
}
