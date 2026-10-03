using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Json;

namespace System.Text.Json.Serialization;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a JSON type resolver allowing to serialize a given type from its base type.
    /// </summary>
    public static IServiceCollection AddJsonDerivedTypeInfo<TDerived, TBase>(this IServiceCollection services)
        where TDerived : class
        where TBase : class
    {
        return services.Configure<JsonDerivedTypesOptions>(options =>
        {
            if (!options.DerivedTypes.TryGetValue(typeof(TBase), out var derivedTypes))
            {
                derivedTypes = [];
                options.DerivedTypes[typeof(TBase)] = derivedTypes;
            }

            derivedTypes.Add(new JsonDerivedTypeInfo<TDerived, TBase>());
        });
    }

    /// <summary>
    /// Registers a concrete fallback type for an abstract or interface base type. When the JSON
    /// deserializer encounters an unrecognized type discriminator for <typeparamref name="TBase"/>,
    /// it will deserialize as <typeparamref name="TFallback"/> instead of throwing.
    /// </summary>
    public static IServiceCollection AddJsonDerivedTypeFallback<TBase, TFallback>(this IServiceCollection services)
        where TBase : class
        where TFallback : TBase, IUnknownTypePlaceholder, new()
    {
        return services.Configure<JsonDerivedTypesOptions>(options =>
                options.FallbackTypes[typeof(TBase)] = typeof(TFallback));
    }
}
