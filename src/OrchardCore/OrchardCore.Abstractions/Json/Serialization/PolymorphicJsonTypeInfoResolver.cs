using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json.Serialization;

public class PolymorphicJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    private readonly FrozenDictionary<Type, JsonDerivedType[]> _derivedTypes;

    public PolymorphicJsonTypeInfoResolver(IEnumerable<IJsonDerivedTypeInfo> derivedTypes)
    {
        _derivedTypes = derivedTypes
            .GroupBy(info => info.BaseType)
            .ToFrozenDictionary(
                group => group.First().BaseType,
                group => group.Select(info => info.DerivedType).ToArray());
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        // If there is not derived type to add to this type, return the standard one.

        if (!_derivedTypes.TryGetValue(jsonTypeInfo.Type, out var jsonDerivedTypes))
        {
            return jsonTypeInfo;
        }

        // At that point we need to list the potential sub-classes that
        // this type could also represent.

        jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$type",
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };

        foreach (var derivedType in jsonDerivedTypes)
        {
            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType);
        }

        // We don't need to cache the result since the serializer does that already.
        // Meaning this code is not called twice for the same base type.

        return jsonTypeInfo;
    }
}
