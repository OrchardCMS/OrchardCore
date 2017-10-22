using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.Queries;

namespace OrchardCore.Apis.GraphQL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphQL(this IServiceCollection services)
        {
            services.AddScoped<IDocumentExecuter, DocumentExecuter>();

            //services.AddGraphQueryType<TitlePartType>();
            //services.AddGraphQueryType<AutoRoutePartType>();
            //services.AddGraphQueryType<BagPartType>();

            services.AddGraphQLQueries();
            services.AddGraphQLMutations();

            // Schema
            services.AddScoped<ISchema, ContentSchema>();
        
            return services;
        }
    }
}
