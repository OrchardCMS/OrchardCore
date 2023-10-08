using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Workflows.Abstractions.Converters
{
    // TODO: Should this be moved to a more central library?
    /// <summary>
    /// Serializes the <see cref="LocalizedString"/> to a simple string using the translated text.
    /// </summary>
    public class LocalizedStringConverter : JsonConverter<LocalizedString>
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(LocalizedString);

        public override LocalizedString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, LocalizedString value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.Value);
    }
}
