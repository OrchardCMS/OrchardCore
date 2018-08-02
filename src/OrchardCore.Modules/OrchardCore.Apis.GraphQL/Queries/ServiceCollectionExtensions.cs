using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphQLQueries(this IServiceCollection services)
        {
            services.AddTransient<QueriesSchema>();
        }
    }
}
