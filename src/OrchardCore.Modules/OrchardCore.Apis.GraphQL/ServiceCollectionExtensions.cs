using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Apis.GraphQL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphQL(this IServiceCollection services)
        {
            services.AddScoped<IDependencyResolver, InternalDependencyResolver>();
            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddSingleton<IDocumentWriter, DocumentWriter>();
            
            services.AddGraphQLQueries();
            services.AddGraphQLMutations();

            // Schema
            services.AddScoped<ISchema, ContentSchema>();
        
            return services;
        }
    }
}
