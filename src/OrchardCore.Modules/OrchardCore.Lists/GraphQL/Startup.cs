using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Modules;

namespace OrchardCore.Lists.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQLInputType<ContainedPart, ContainedInputObjectType>();
            services.AddGraphQLQueryType<ContainedPart, ContainedQueryObjectType>();
            services.AddGraphQLQueryArgumentInputType<ContainedInputObjectType>();
            services.AddGraphQLFilterType<ContentItem, ContainedGraphQLFilter>();
        }
    }
}
