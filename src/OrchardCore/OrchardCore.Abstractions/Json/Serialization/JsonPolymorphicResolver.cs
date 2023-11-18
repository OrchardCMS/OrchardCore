using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json.Serialization;

public class JsonPolymorphicResolver : DefaultJsonTypeInfoResolver
{
    private readonly IDictionary<Type, JsonDerivedType[]> _derivedTypes;

    public JsonPolymorphicResolver(IEnumerable<IJsonDerivedTypeInfo> derivedTypes)
    {
        _derivedTypes = derivedTypes
            .GroupBy(info => info.BaseType)
            .ToDictionary(
                group => group.First().BaseType,
                group => group.Select(info => info.DerivedType).ToArray());
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);
        if (!_derivedTypes.TryGetValue(jsonTypeInfo.Type, out var jsonDerivedTypes))
        {
            return jsonTypeInfo;
        }

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

        return jsonTypeInfo;
    }
}
