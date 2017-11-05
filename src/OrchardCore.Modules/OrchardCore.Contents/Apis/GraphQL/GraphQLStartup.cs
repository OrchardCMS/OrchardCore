using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Contents.Apis.GraphQL.Mutations;
using OrchardCore.Contents.Apis.GraphQL.Mutations.Types;
using OrchardCore.Contents.Apis.GraphQL.Queries;
using OrchardCore.Contents.Apis.GraphQL.Queries.Providers;
using OrchardCore.Contents.Apis.GraphQL.Queries.Types;
using OrchardCore.Contents.Apis.GraphQL.Schema;
using OrchardCore.Modules;

namespace OrchardCore.Setup.Apis.GraphQL
{
    public class GraphQLStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphMutationType<CreateContentItemMutation>();
            services.AddGraphMutationType<DeleteContentItemMutation>();
            services.AddScoped<ContentItemInputType>();

            services.AddGraphQueryType<ContentItemQuery>();
            services.AddGraphQueryType<ContentItemsQuery>();
            services.AddScoped<ContentItemType>();

            services.AddScoped<IDynamicQueryFieldTypeProvider, DynamicQueryFieldTypeProvider>();

            services.AddScoped<IObjectGraphTypeProvider, ObjectGraphTypeProvider>();
        }
    }
}
