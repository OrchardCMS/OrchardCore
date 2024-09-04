using System.Text.Json.Serialization;

namespace OrchardCore.Json;

public class JsonDerivedTypesOptions
{
    internal Dictionary<Type, List<IJsonDerivedTypeInfo>> DerivedTypes { get; } = [];

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
}
