using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphQLQueries(this IServiceCollection services)
        {
            services.AddGraphQueryType<ContentItemQuery>();
            services.AddGraphQueryType<ContentItemsQuery>();
            services.AddScoped<ContentItemType>();

            services.AddScoped<IDynamicQueryFieldTypeProvider, DynamicQueryFieldTypeProvider>();
            services.AddScoped<QueriesSchema>();
        }
    }
}
