using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.RestApis.Filters;
using OrchardCore.RestApis.GraphQL.Mutations;
using OrchardCore.RestApis.GraphQL.Queries;
using OrchardCore.RestApis.Queries;
using OrchardCore.RestApis.Queries.Types;
using OrchardCore.RestApis.Types;

namespace OrchardCore.RestApis
{
    public static class GraphQLServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphQL(this IServiceCollection services)
        {
            services.AddScoped<IDocumentExecuter, DocumentExecuter>();

            //services.AddGraphQueryType<TitlePartType>();
            //services.AddGraphQueryType<AutoRoutePartType>();
            //services.AddGraphQueryType<BagPartType>();

            //services.AddScoped<ContentItemType>();
            //services.AddScoped<ContentTypeType>();
            //services.AddScoped<ContentType>();

            //services.AddScoped<AutoRoutePartType>();
            //services.AddScoped<IObjectGraphType, AutoRoutePartType>();
            //services.AddScoped<ContentPartInterface>();

            services.AddGraphQueryType<ContentItemQuery>();
            services.AddGraphQueryType<ContentItemsQuery>();

            services.AddGraphMutationType<CreateContentItemMutation>();

            services.AddScoped<ISchema, ContentSchema>();
            
            services.AddScoped<ContentItemInputType>();
            services.AddScoped<ContentItemType>();

            services.AddScoped<GraphQlQueryType>();

            return services;
        }

        public static void AddGraphQueryType<T>(this IServiceCollection services) where T : QueryFieldType
        {
            services.AddScoped<T>();
            services.AddScoped<QueryFieldType, T>();
        }

        public static void AddGraphMutationType<T>(this IServiceCollection services) where T : MutationFieldType
        {
            services.AddScoped<T>();
            services.AddScoped<MutationFieldType, T>();
        }

    }
}
