#nullable enable

namespace System.Text.Json.Nodes;

public static class JsonNodeExtensions
{
    /// <summary>
    /// Attempts to get the value of the specified type from this <see cref="JsonNode"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to get.</typeparam>
    /// <param name="node">The node to get the value from.</param>
    /// <param name="value">When this method returns, contains the value if successful; otherwise, the default value.</param>
    /// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
    public static bool TryGetValue<T>(this JsonNode? node, out T? value)
    {
        if (node is JsonValue jsonValue && jsonValue.TryGetValue<T>(out var result))
        {
            value = result;
            return true;
        }

        value = default;
        return false;
    }

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

    public static bool TryGetEnumValue<TEnum>(this JsonNode node, out TEnum? value)
        where TEnum : struct, Enum
    {
        value = node.GetEnumValue<TEnum>();

        return value != null;
    }
}
