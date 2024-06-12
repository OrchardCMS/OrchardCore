using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Json;
using OrchardCore.Json.Dynamic;

namespace System.Text.Json.Serialization;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a JSON type resolver allowing to serialize a given type from its base type.
    /// </summary>
    public static IServiceCollection AddJsonDerivedTypeInfo<TDerived, TBase>(this IServiceCollection services)
        where TDerived : class
        where TBase : class
        => services.Configure<JsonDerivedTypesOptions>(options =>
        {
            if (!options.DerivedTypes.TryGetValue(typeof(TBase), out var derivedTypes))
            {
                derivedTypes = [];
                options.DerivedTypes[typeof(TBase)] = derivedTypes;
            }

            derivedTypes.Add(new JsonDerivedTypeInfo<TDerived, TBase>());
        });

    public static IServiceCollection AddJsonDynamicValueHandler<T>(this IServiceCollection services) where T : IJsonDynamicValueHandler, new()
    {
        if (JsonDynamicConfigurations.ValueHandlers.Any(x => x.GetType() != typeof(T)))
        {
            JsonDynamicConfigurations.ValueHandlers.Add(new T());
        }
        return services;
    }
}
