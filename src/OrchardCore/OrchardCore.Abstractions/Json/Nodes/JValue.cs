#nullable enable

using Microsoft.Extensions.Options;

namespace System.Text.Json.Nodes;

public static class JValue
{
    /// <summary>
    /// Creates a <see cref="JsonValue"/> from an object.
    /// </summary>
    public static JsonValue? FromObject(object? obj, JsonSerializerOptions? options = null) =>
        JsonValue.Create(JsonSerializer.SerializeToElement(obj, options ?? JNode.Options));

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonValue"/>.
    /// </summary>
    public static JsonValue? Clone(this JsonValue? value) => value?.DeepClone().AsValue();
}
