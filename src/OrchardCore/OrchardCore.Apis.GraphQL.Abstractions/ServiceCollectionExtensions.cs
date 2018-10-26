using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Apis
{
    public static class ServiceCollectionExtensions
    {
        //public static void AddGraphQLInputType<TInput, TInputType>(this IServiceCollection services) 
        //    where TInput : class 
        //    where TInputType : InputObjectGraphType<TInput>
        //{
        //    services.AddTransient<InputObjectGraphType<TInput>, TInputType>();
        //    services.AddTransient<IInputObjectGraphType, TInputType>();
        //}

        /// <summary>
        /// Registers a type describing input arguments
        /// </summary>
        /// <typeparam name="TInputType"></typeparam>
        /// <param name="services"></param>
        public static void AddInputObjectGraphType<TObject, TObjectType>(this IServiceCollection services)
            where TObject : class 
            where TObjectType : InputObjectGraphType<TObject>
        {
            services.AddTransient<TObjectType>();
            services.AddTransient<InputObjectGraphType<TObject>, TObjectType>();
            services.AddTransient<IInputObjectGraphType, TObjectType>();
        }

        /// <summary>
        /// Registers a type describing output arguments
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TInputType"></typeparam>
        /// <param name="services"></param>
        public static void AddObjectGraphType<TInput, TInputType>(this IServiceCollection services)
            where TInput : class
            where TInputType : ObjectGraphType<TInput>
        {
            services.AddTransient<TInputType>();
            services.AddTransient<ObjectGraphType<TInput>, TInputType>();
            services.AddTransient<IObjectGraphType, TInputType>();
        }

        /// <summary>
        /// Registers a type providing custom filters for content item filters
        /// </summary>
        /// <typeparam name="TObjectTypeToFilter"></typeparam>
        /// <typeparam name="TFilterType"></typeparam>
        /// <param name="services"></param>
        public static void AddGraphQLFilterType<TObjectTypeToFilter, TFilterType>(this IServiceCollection services)
            where TObjectTypeToFilter : class
            where TFilterType : GraphQLFilter<TObjectTypeToFilter>
        {
            services.AddTransient<IGraphQLFilter<TObjectTypeToFilter>, TFilterType>();
        }
    }
}
