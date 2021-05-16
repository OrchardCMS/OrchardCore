using System;
using OrchardCore.ResourceManagement.TagHelpers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds Orchard CMS services to the application.
        /// </summary>
        public static OrchardCoreBuilder AddOrchardCms(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var builder = services.AddOrchardCore()

                .AddCommands()

                .AddSecurity()
                .AddMvc()
                .AddIdGeneration()
                .AddEmailAddressValidator()
                .AddHtmlSanitizer()
                .AddSetupFeatures("OrchardCore.Setup")

                .AddDataAccess()
                .AddDataStorage()
                .AddBackgroundService()
                .AddScripting()

                .AddTheming()
                .AddLiquidViews()
                .AddCaching();

            // OrchardCoreBuilder is not available in OrchardCore.ResourceManagement as it has to
            // remain independent from OrchardCore.
            builder.ConfigureServices(s =>
            {
                s.AddResourceManagement();

                s.AddTagHelpers<LinkTagHelper>();
                s.AddTagHelpers<MetaTagHelper>();
                s.AddTagHelpers<ResourcesTagHelper>();
                s.AddTagHelpers<ScriptTagHelper>();
                s.AddTagHelpers<StyleTagHelper>();
            });

            return builder;
        }

        /// <summary>
        /// Adds Orchard CMS services to the application and let the app change the
        /// default tenant behavior and set of features through a configure action.
        /// </summary>
        public static IServiceCollection AddOrchardCms(this IServiceCollection services, Action<OrchardCoreBuilder> configure)
        {
            var builder = services.AddOrchardCms();

            configure?.Invoke(builder);

            return services;
        }
    }
}
