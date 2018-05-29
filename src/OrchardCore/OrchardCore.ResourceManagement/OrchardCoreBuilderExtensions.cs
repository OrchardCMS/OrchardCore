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
            builder.Startup.ConfigureServices((tenant, sp) =>
            {
                tenant.AddResourceManagement();
                tenant.Services.AddTagHelpers(typeof(ResourcesTagHelper).Assembly);
            });

            return builder;
        }
    }
}
