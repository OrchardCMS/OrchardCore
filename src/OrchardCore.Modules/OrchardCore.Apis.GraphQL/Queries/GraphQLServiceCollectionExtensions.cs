using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public static class GraphQLServiceCollectionExtensions
    {
        public static void AddGraphQLQueries(this IServiceCollection services)
        {
            services.AddScoped<QueriesSchema>();
            services.AddGraphQueryType<ContentItemQuery>();
            services.AddGraphQueryType<ContentItemsQuery>();
            services.AddScoped<IDynamicQueryFieldTypeProvider, DynamicQueryFieldTypeProvider>();
            services.AddScoped<ContentItemType>();
        }

        public static void AddGraphQueryType<T>(this IServiceCollection services) where T : QueryFieldType
        {
            services.AddScoped<T>();
            services.AddScoped<QueryFieldType, T>();
        }
    }
}
