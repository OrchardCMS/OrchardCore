using OrchardCore.Apis;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Modules;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.GraphQL;

[RequireFeatures("OrchardCore.Apis.GraphQL")]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IContentFieldProvider, GeoPointFieldProvider>();
        services.AddObjectGraphType<GeoPointField, GeoPointFieldQueryObjectType>();
    }
}
