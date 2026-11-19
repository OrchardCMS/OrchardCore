using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.Json.Serialization;

/// <summary>
/// Serializes a <see cref="LocalizationSource"/> as its untranslated <see cref="LocalizationSource.Value"/>.
/// </summary>
/// <remarks>
/// <see cref="LocalizationSource.Type"/> is not written, because <see cref="Type"/> instances can't be serialized, and
/// a source read back has no source type, because creating a type from a name read from a document isn't safe. A source
/// that must keep its localization context is read from code, for example from an override marked with
/// <see cref="JsonIgnoreAttribute"/>, instead of being persisted.
/// </remarks>
[SkipLocalizationExtraction]
public sealed class LocalizationSourceJsonConverter : JsonConverter<LocalizationSource>
{
    public static readonly LocalizationSourceJsonConverter Instance = new();

    public override LocalizationSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a string for a {nameof(LocalizationSource)}, but found '{reader.TokenType}'.");
        }

        return LocalizationSource.Create(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, LocalizationSource value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
