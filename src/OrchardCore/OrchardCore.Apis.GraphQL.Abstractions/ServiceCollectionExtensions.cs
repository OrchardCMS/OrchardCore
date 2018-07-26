using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphMutationType<TMutation>(this IServiceCollection services) where TMutation : MutationFieldType
        {
            services.AddScoped<TMutation>();
            services.AddScoped<MutationFieldType, TMutation>();
        }

        public static void AddGraphQueryType<TQuery>(this IServiceCollection services) where TQuery : QueryFieldType
        {
            services.AddScoped<TQuery>();
            services.AddScoped<QueryFieldType, TQuery>();
        }

        public static void AddGraphQLInputType<TInput, TInputType>(this IServiceCollection services) 
            where TInput : class 
            where TInputType : InputObjectGraphType<TInput>
        {
            services.AddScoped<InputObjectGraphType<TInput>, TInputType>();
            services.AddScoped<IInputObjectGraphType, TInputType>();
        }

        public static void AddGraphQLQueryArgumentInputType<TInputType>(this IServiceCollection services)
            where TInputType : class, IQueryArgumentObjectGraphType
        {
            services.AddScoped<IQueryArgumentObjectGraphType, TInputType>();
        }

        public static void AddGraphQLQueryType<TInput, TInputType>(this IServiceCollection services)
            where TInput : class
            where TInputType : ObjectGraphType<TInput>
        {
            services.AddScoped<ObjectGraphType<TInput>, TInputType>();
            services.AddScoped<IObjectGraphType, TInputType>();
        }

        public static void AddGraphQLFilterType<TObjectTypeToFilter, TFilterType>(this IServiceCollection services)
            where TObjectTypeToFilter : class
            where TFilterType : GraphQLFilter<TObjectTypeToFilter>
        {
            services.AddScoped<IGraphQLFilter<TObjectTypeToFilter>, TFilterType>();
        }
    }
}
