using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Spatial.Drivers;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.Indexing;
using OrchardCore.Spatial.Model;
using OrchardCore.Spatial.Settings;
using OrchardCore.Spatial.ViewModels;

namespace OrchardCore.Spatial
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<GeoPointPartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {

            // Coordinate Field
            services.AddSingleton<ContentField, CoordinateField>();
            services.AddScoped<IContentFieldDisplayDriver, CoordinateFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, CoordinateFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, CoordinateFieldIndexHandler>();

            services.AddSingleton<ContentPart, GeoPointPart>();

            services.AddScoped<IContentPartDisplayDriver, GeoPointPartDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
