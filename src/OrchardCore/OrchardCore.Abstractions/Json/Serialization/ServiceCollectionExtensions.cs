using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace System.Text.Json.Serialization;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a JSON type resolver allowing to serialize a given type from its base type.
    /// </summary>
    public static IServiceCollection AddJsonPolymorphicResolver<TDerived, TBase>(this IServiceCollection services)
        where TDerived : class where TBase : class =>
        services.AddSingleton<IJsonTypeInfoResolver, JsonPolymorphicResolver<TDerived, TBase>>();
}
