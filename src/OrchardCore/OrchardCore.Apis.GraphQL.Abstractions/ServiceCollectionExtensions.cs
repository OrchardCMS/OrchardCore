using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphMutationType<T>(this IServiceCollection services) where T : MutationFieldType
        {
            services.AddScoped<T>();
            services.AddScoped<MutationFieldType, T>();
        }

        public static void AddGraphQueryType<T>(this IServiceCollection services) where T : QueryFieldType
        {
            services.AddScoped<T>();
            services.AddScoped<QueryFieldType, T>();
        }

        public static void AddGraphQLInputType<TContenPart, TInputType>(this IServiceCollection services) 
            where TContenPart : class 
            where TInputType : InputObjectGraphType<TContenPart>
        {
            services.AddScoped<InputObjectGraphType<TContenPart>, TInputType>();
            services.AddScoped<IInputObjectGraphType, TInputType>();
        }

        public static void AddGraphQLQueryArgumentInputType<TInputType>(this IServiceCollection services)
            where TInputType : class, IQueryArgumentObjectGraphType
        {
            services.AddScoped<IQueryArgumentObjectGraphType, TInputType>();
        }

        public static void AddGraphQLQueryType<TContenPart, TInputType>(this IServiceCollection services)
            where TContenPart : class
            where TInputType : ObjectGraphType<TContenPart>
        {
            services.AddScoped<ObjectGraphType<TContenPart>, TInputType>();
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
