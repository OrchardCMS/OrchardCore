using System.Text.Json.Serialization.Metadata;
using OrchardCore.Json;

namespace System.Text.Json.Serialization;

public class PolymorphicJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    private readonly JsonDerivedTypesOptions _derivedTypesOptions;

    public PolymorphicJsonTypeInfoResolver(JsonDerivedTypesOptions derivedTypesOptions)
    {
        _derivedTypesOptions = derivedTypesOptions;
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        // If there is not derived type to add to this type, return the standard one.

        if (!_derivedTypesOptions.TryGetDerivedTypes(jsonTypeInfo.Type, out var jsonDerivedTypes))
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
            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(derivedType.DerivedType);
        }

        // We don't need to cache the result since the serializer does that already.
        // Meaning this code is not called twice for the same base type.

        return jsonTypeInfo;
    }
}
