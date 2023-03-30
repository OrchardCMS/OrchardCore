using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Spatial.Drivers;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.Handlers;
using OrchardCore.Spatial.Indexing;
using OrchardCore.Spatial.ViewModels;

namespace OrchardCore.Spatial
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();

            // Coordinate Field
            services.AddContentField<GeoPointField>()
                .UseDisplayDriver<GeoPointFieldDisplayDriver>()
                .AddHandler<GeoPointFieldHandler>();

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GeoPointFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, GeoPointFieldIndexHandler>();

            // Registering both field types and shape types are necessary as they can
            // be accessed from inner properties.
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<GeoPointField>();
                o.MemberAccessStrategy.Register<DisplayGeoPointFieldViewModel>();
            });
        }
    }
}
