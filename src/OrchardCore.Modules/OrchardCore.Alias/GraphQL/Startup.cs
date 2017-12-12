using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Alias.Models;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;

namespace OrchardCore.Alias.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQLInputType<AliasPart, AliasInputObjectType>();
            services.AddGraphQLQueryType<AliasPart, AliasQueryObjectType>();
            services.AddGraphQLQueryArgumentInputType<AliasInputObjectType>();
            services.AddGraphQLFilterType<ContentItem, AliasGraphQLFilter>();
        }
    }
}
