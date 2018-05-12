using System;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace OrchardCore.Workflows.Abstractions.Converters
{
    // TODO: Should this be moved to a more central library?
    /// <summary>
    /// Serializes the <see cref="LocalizedString"/> to a simple string using the translated text.
    /// </summary>
    public class LocalizedStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LocalizedString);
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var localizedString = (LocalizedString)value;
            writer.WriteValue(localizedString.Value);
        }
    }
}
