using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Seo.Models;

namespace OrchardCore.Seo.GraphQL;

[RequireFeatures("OrchardCore.Apis.GraphQL")]
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddObjectGraphType<MetaEntry, MetaEntryQueryObjectType>();
        services.AddObjectGraphType<SeoMetaPart, SeoMetaQueryObjectType>();
    }
}
