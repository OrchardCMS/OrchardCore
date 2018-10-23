using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Apis
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphQLInputType<TInput, TInputType>(this IServiceCollection services) 
            where TInput : class 
            where TInputType : InputObjectGraphType<TInput>
        {
            services.AddTransient<InputObjectGraphType<TInput>, TInputType>();
            services.AddTransient<IInputObjectGraphType, TInputType>();
        }

        public static void AddGraphQLQueryArgumentInputType<TInputType>(this IServiceCollection services)
            where TInputType : class, IQueryArgumentObjectGraphType
        {
            services.AddTransient<IQueryArgumentObjectGraphType, TInputType>();
        }

        public static void AddGraphQLQueryType<TInput, TInputType>(this IServiceCollection services)
            where TInput : class
            where TInputType : ObjectGraphType<TInput>
        {
            services.AddTransient<ObjectGraphType<TInput>, TInputType>();
            services.AddTransient<IObjectGraphType, TInputType>();
        }

        public static void AddGraphQLFilterType<TObjectTypeToFilter, TFilterType>(this IServiceCollection services)
            where TObjectTypeToFilter : class
            where TFilterType : GraphQLFilter<TObjectTypeToFilter>
        {
            services.AddTransient<IGraphQLFilter<TObjectTypeToFilter>, TFilterType>();
        }
    }
}
