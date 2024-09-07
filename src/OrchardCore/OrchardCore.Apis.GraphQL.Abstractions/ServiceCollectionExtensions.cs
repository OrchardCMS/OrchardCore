using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Apis;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a type describing input arguments.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TObjectType"></typeparam>
    /// <param name="services"></param>
    public static void AddInputObjectGraphType<TObject, TObjectType>(this IServiceCollection services)
        where TObject : class
        where TObjectType : InputObjectGraphType<TObject>
    {
        services.AddTransient<TObjectType>();
        services.AddTransient<InputObjectGraphType<TObject>, TObjectType>(s => s.GetRequiredService<TObjectType>());
        services.AddTransient<IInputObjectGraphType, TObjectType>(s => s.GetRequiredService<TObjectType>());
    }

    /// <summary>
    /// Registers a type describing output arguments.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TInputType"></typeparam>
    /// <param name="services"></param>
    public static void AddObjectGraphType<TInput, TInputType>(this IServiceCollection services)
        where TInput : class
        where TInputType : ObjectGraphType<TInput>
    {
        services.AddTransient<TInputType>();
        services.AddTransient<ObjectGraphType<TInput>, TInputType>(s => s.GetRequiredService<TInputType>());
        services.AddTransient<IObjectGraphType, TInputType>(s => s.GetRequiredService<TInputType>());
    }
}
