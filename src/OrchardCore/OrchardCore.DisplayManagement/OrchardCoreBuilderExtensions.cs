using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host and tenant level services for managing themes.
        /// </summary>
        public static OrchardCoreBuilder AddTheming(this OrchardCoreBuilder builder)
        {
            builder.Services.AddThemingHost();

            builder.AddManifestDefinition("theme")
                .Startup.ConfigureServices((collection, sp) =>
                {
                    collection.AddTheming();
                    collection.AddTagHelpers(typeof(BaseShapeTagHelper).Assembly);
                });

            return builder;
        }
    }
}
