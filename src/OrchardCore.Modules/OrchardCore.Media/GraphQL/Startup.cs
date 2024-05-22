using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Media.Fields;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Seo.GraphQL;
using OrchardCore.Seo.Models;

namespace OrchardCore.Media.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISchemaBuilder, MediaAssetQuery>();
            services.AddObjectGraphType<MediaField, MediaFieldQueryObjectType>();
            services.AddTransient<MediaAssetObjectType>();
        }
    }

    [RequireFeatures("OrchardCore.Apis.GraphQL", "OrchardCore.Seo")]
    public class SeoStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddObjectGraphType<MetaEntry, MetaEntryQueryObjectType>();
            services.AddObjectGraphType<SeoMetaPart, SeoMetaQueryObjectType>();
        }
    }
}
