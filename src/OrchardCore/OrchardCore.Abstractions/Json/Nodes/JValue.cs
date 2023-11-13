namespace System.Text.Json.Nodes;

#nullable enable

public static class JValue
{
    /// <summary>
    /// Parses text representing a single JSON value.
    /// </summary>
    public static JsonValue? Parse(string json) => JNode.Parse(json)?.AsValue();

    /// <summary>
    /// Parses text representing a single JSON value.
    /// </summary>
    public static JsonValue? Parse(string json, JsonNodeOptions? nodeOptions = null, JsonDocumentOptions documentOptions = default)
        => JNode.Parse(json, nodeOptions, documentOptions)?.AsValue();

    /// <summary>
    /// Creates a <see cref="JsonValue"/> from an object.
    /// </summary>
    public static JsonValue? FromObject(object? obj, JsonSerializerOptions? options = null)
    {
        if (obj is JsonValue jsonValue)
        {
            return jsonValue;
        }

        if (obj is JsonElement jsonElement)
        {
            return JsonValue.Create(jsonElement, JsonOptions.Node);
        }

        return JsonValue.Create(JsonSerializer.SerializeToElement(obj, options ?? JsonOptions.Default), JsonOptions.Node);
    }

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonValue"/>.
    /// </summary>
    public static JsonValue? Clone(this JsonValue? value) => value?.DeepClone().AsValue();
}
