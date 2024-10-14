using Json.Path;

namespace System.Text.Json.Nodes;

public static class JsonNodeExtensions
{
    public static TEnum? GetEnumValue<TEnum>(this JsonNode node)
        where TEnum : struct, Enum
    {
        if (node.TryGetValue<string>(out var stringValue) && Enum.TryParse<TEnum>(stringValue, out var value))
        {
            return value;
        }

        if (node.TryGetValue<int>(out var intValue) && Enum.IsDefined(typeof(TEnum), intValue))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
        }

        return default;
    }

    public static bool TryGetEnumValue<TEnum>(this JsonNode node, out Enum value)
        where TEnum : struct, Enum
    {
        value = node.GetEnumValue<TEnum>();

        return value != null;
    }
}
