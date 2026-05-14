using System.Text.Json.Serialization;

namespace OrchardCore.Json;

public class JsonDerivedTypesOptions
{
    internal Dictionary<Type, List<IJsonDerivedTypeInfo>> DerivedTypes { get; } = [];

    internal Dictionary<Type, Type> FallbackTypes { get; } = [];

    public bool TryGetDerivedTypes(Type type, out IEnumerable<IJsonDerivedTypeInfo> derivedTypes)
    {
        if (!DerivedTypes.TryGetValue(type, out var list))
        {
            derivedTypes = [];
            return false;
        }

        derivedTypes = list;

        return true;
    }

    public bool TryGetFallbackType(Type baseType, out Type fallbackType)
    {
        return FallbackTypes.TryGetValue(baseType, out fallbackType);
    }
}
