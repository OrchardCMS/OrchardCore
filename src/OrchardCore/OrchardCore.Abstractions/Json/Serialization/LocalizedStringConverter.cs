using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Json.Serialization;

/// <summary>
/// Serialized <see cref="LocalizedString"/> into JSON object, or deserializes it from string or object JSON nodes.
/// </summary>
/// <remarks><para>
/// This is necessary, because out of the box converting this type will result in an exception saying "The JSON value
/// could not be converted".
/// </para></remarks>
public class LocalizedStringConverter : JsonConverter<LocalizedString>
{
    public override LocalizedString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var node = JsonNode.Parse(ref reader);

        if (node?.GetValueKind() == JsonValueKind.String)
        {
            var text = node.GetValue<string>();
            return new(text, text);
        }

        if (node is JsonObject jsonObject)
        {
            var dictionary = new Dictionary<string, JsonNode>(jsonObject, StringComparer.OrdinalIgnoreCase);

            var name = GetStringNodeValue(dictionary, nameof(LocalizedString.Name));
            var value = GetStringNodeValue(dictionary, nameof(LocalizedString.Value)) ?? name;
            var isResourceNotFound =
                dictionary.TryGetValue(nameof(LocalizedString.ResourceNotFound), out var notFoundNode)
                && notFoundNode?.Deserialize<bool>() == true;
            var searchedLocation = GetStringNodeValue(dictionary, nameof(LocalizedString.SearchedLocation));

            name ??= value;
            if (string.IsNullOrEmpty(name)) throw new InvalidOperationException("Missing name.");

            return new(name, value, isResourceNotFound, searchedLocation);
        }

        throw new InvalidOperationException($"Can't parse token \"{node}\". It should be an object or a string");
    }

    public override void Write(Utf8JsonWriter writer, LocalizedString value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(LocalizedString.Name), value.Name);
        writer.WriteString(nameof(LocalizedString.Value), value.Value);
        writer.WriteBoolean(nameof(LocalizedString.ResourceNotFound), value.ResourceNotFound);
        writer.WriteString(nameof(LocalizedString.SearchedLocation), value.SearchedLocation);

        writer.WriteEndObject();
    }

    internal static string GetStringNodeValue(IDictionary<string, JsonNode> dictionary, string key) =>
        dictionary.TryGetValue(key, out var childNode) ? childNode?.Deserialize<string>() : null;
}

