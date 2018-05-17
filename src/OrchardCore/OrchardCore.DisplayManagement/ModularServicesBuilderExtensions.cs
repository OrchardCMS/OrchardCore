using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        /// Adds host and tenant level services for managing themes.
        /// </summary>
        /// <param name="services"></param>
        public static ModularServicesBuilder AddTheming(this ModularServicesBuilder builder)
        {
            builder.Services.AddThemingHost()
                .AddManifestDefinition("theme")
                .ConfigureTenantServices(collection =>
                {
                    collection.AddTheming();
                });

            return builder;
        }
    }
}
