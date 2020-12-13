using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Apis
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a type describing input arguments
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TObjectType"></typeparam>
        /// <param name="services"></param>
        public static void AddInputObjectGraphType<TObject, TObjectType>(this IServiceCollection services)
            where TObject : class
            where TObjectType : InputObjectGraphType<TObject>
        {
            // Instances are registered as singletons as their constructor holds the logic to configure the type
            // and doesn't need to run every time
            services.AddSingleton<TObjectType>();
            services.AddSingleton<InputObjectGraphType<TObject>, TObjectType>(s => s.GetRequiredService<TObjectType>());
            services.AddSingleton<IInputObjectGraphType, TObjectType>(s => s.GetRequiredService<TObjectType>());
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
            // Instances are registered as singletons as their constructor holds the logic to configure the type
            // and doesn't need to run every time
            services.AddSingleton<TInputType>();
            services.AddSingleton<ObjectGraphType<TInput>, TInputType>(s => s.GetRequiredService<TInputType>());
            services.AddSingleton<IObjectGraphType, TInputType>(s => s.GetRequiredService<TInputType>());
        }
    }
}
