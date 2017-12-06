using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Contents.GraphQL.Mutations;
using OrchardCore.Contents.GraphQL.Mutations.Types;
using OrchardCore.Contents.GraphQL.Queries;
using OrchardCore.Contents.GraphQL.Queries.Providers;
using OrchardCore.Contents.GraphQL.Queries.Types;
using OrchardCore.Modules;

namespace OrchardCore.Contents.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphMutationType<CreateContentItemMutation>();
            services.AddGraphMutationType<DeleteContentItemMutation>();

            services.AddGraphQueryType<ContentItemQuery>();
            services.AddGraphQueryType<ContentItemsQuery>();
            services.AddScoped<ContentItemType>();
            services.AddScoped<DeletionStatusObjectGraphType>();
            services.AddScoped<CreateContentItemInputType>();
            services.AddScoped<ContentPartsInputType>();

            services.AddScoped<IDynamicQueryFieldTypeProvider, ContentItemFieldTypeProvider>();
        }
    }
}
