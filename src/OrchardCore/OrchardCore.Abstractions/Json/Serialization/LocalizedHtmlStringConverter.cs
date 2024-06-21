using Microsoft.AspNetCore.Mvc.Localization;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.Json.Serialization;

/// <summary>
/// Serialized <see cref="LocalizedHtmlString"/> into JSON object, or deserializes it from string or object JSON nodes.
/// </summary>
/// <remarks><para>
/// This is necessary, because out of the box converting this type will result in an exception saying "The JSON value
/// could not be converted".
/// </para></remarks>
public class LocalizedHtmlStringConverter : JsonConverter<LocalizedHtmlString>
{
    public override LocalizedHtmlString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            var name = LocalizedStringConverter.GetStringNodeValue(dictionary, nameof(LocalizedHtmlString.Name));
            var value = LocalizedStringConverter.GetStringNodeValue(dictionary, nameof(LocalizedHtmlString.Value)) ?? name;
            var isResourceNotFound =
                dictionary.TryGetValue(nameof(LocalizedHtmlString.IsResourceNotFound), out var notFoundNode)
                && notFoundNode?.Deserialize<bool>() == true;

            name ??= value;
            if (string.IsNullOrEmpty(name)) throw new InvalidOperationException("Missing name.");

            return new(name, value, isResourceNotFound);
        }

        throw new InvalidOperationException($"Can't parse token \"{node}\". It should be an object or a string");
    }

    public override void Write(Utf8JsonWriter writer, LocalizedHtmlString value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(LocalizedHtmlString.Name), value.Name);
        writer.WriteString(nameof(LocalizedHtmlString.Value), value.Value);
        writer.WriteBoolean(nameof(LocalizedHtmlString.IsResourceNotFound), value.IsResourceNotFound);

        writer.WriteEndObject();
    }
}
