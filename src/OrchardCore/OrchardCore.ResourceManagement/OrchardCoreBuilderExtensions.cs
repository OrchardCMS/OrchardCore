using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ResourceManagement;
using OrchardCore.ResourceManagement.Filters;
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
            builder.ConfigureServices(services =>
            {
                services.TryAddScoped<IResourceManager, ResourceManager>();
                services.TryAddScoped<IRequireSettingsProvider, DefaultRequireSettingsProvider>();
                services.TryAddSingleton<IResourceManifestState, ResourceManifestState>();

                services.AddTagHelpers(typeof(ResourcesTagHelper).Assembly);
            });

            return builder;
        }

        /// <summary>
        /// Adds a tenant level mvc filter which registers a Generator Meta Tag.
        /// </summary>
        public static OrchardCoreBuilder AddGeneratorTagFilter(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<MvcOptions>((options) =>
                {
                    options.Filters.Add(typeof(MetaGeneratorFilter));
                });
            });

            return builder;
        }
    }
}
