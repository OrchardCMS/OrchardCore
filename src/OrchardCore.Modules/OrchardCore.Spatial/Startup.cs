using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Spatial.Drivers;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.Indexing;
using OrchardCore.Spatial.Settings;
using OrchardCore.Spatial.ViewModels;
using GeoPointField = OrchardCore.Spatial.Fields.GeoPointField;

namespace OrchardCore.Spatial
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            // Registering both field types and shape types are necessary as they can
            // be accessed from inner properties.

            TemplateContext.GlobalMemberAccessStrategy.Register<GeoPointField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayGeoPointFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();

            // Coordinate Field
            services.AddSingleton<ContentField, GeoPointField>();
            services.AddScoped<IContentFieldDisplayDriver, GeoPointFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GeoPointFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, GeoPointFieldIndexHandler>();
        }
    }
}
