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

namespace OrchardCore.Spatial
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            // Registering both field types and shape types are necessary as they can
            // be accessed from inner properties.

            TemplateContext.GlobalMemberAccessStrategy.Register<CoordinateField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayCoordinateFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();

            // Coordinate Field
            services.AddSingleton<ContentField, CoordinateField>();
            services.AddScoped<IContentFieldDisplayDriver, CoordinateFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, CoordinateFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, CoordinateFieldIndexHandler>();
        }
    }
}
