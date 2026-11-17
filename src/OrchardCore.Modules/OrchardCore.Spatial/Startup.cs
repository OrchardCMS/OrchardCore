using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Spatial.Drivers;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.Handlers;
using OrchardCore.Spatial.Indexing;

namespace OrchardCore.Spatial;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        // Coordinate Field
        services.AddContentField<GeoPointField>()
            .UseDisplayDriver<GeoPointFieldDisplayDriver>()
            .AddHandler<GeoPointFieldHandler>();

        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, GeoPointFieldSettingsDriver>();
        services.AddScoped<IContentFieldIndexHandler, GeoPointFieldIndexHandler>();

        // Registering both field types and shape types are necessary as they can
        // be accessed from inner properties.
    }
}
