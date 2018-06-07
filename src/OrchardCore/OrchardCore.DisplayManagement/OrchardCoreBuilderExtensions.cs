using OrchardCore.DisplayManagement.Events;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host and tenant level services for managing themes.
        /// </summary>
        public static OrchardCoreBuilder AddTheming(this OrchardCoreBuilder builder)
        {
            builder.AddThemingHost()
                .AddManifestDefinition("theme")
                .Startup.ConfigureServices(tenant =>
                {
                    tenant.AddTheming();
                    tenant.Services.AddTagHelpers(typeof(BaseShapeTagHelper).Assembly);
                });

            return builder;
        }

        /// <summary>
        /// Adds host level services for managing themes.
        /// </summary>
        public static OrchardCoreBuilder AddThemingHost(this OrchardCoreBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IExtensionDependencyStrategy, ThemeExtensionDependencyStrategy>();
            services.AddSingleton<IFeatureBuilderEvents, ThemeFeatureBuilderEvents>();

            return builder;
        }
    }
}
