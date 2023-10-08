using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Notify
{
    public class NotifyEntryConverter : JsonConverter<NotifyEntry>
    {
        private readonly HtmlEncoder _htmlEncoder;

        public NotifyEntryConverter(HtmlEncoder htmlEncoder)
        {
            _htmlEncoder = htmlEncoder;
        }

        public override bool CanConvert(Type typeToConvert) => typeof(NotifyEntry).IsAssignableFrom(typeToConvert);

        public override NotifyEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var notifyEntry = new NotifyEntry();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return notifyEntry;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch(propertyName)
                {
                    case nameof(notifyEntry.Type):
                        if (Enum.TryParse(reader.GetString(), out NotifyType type))
                        {
                            notifyEntry.Type = type;
                        }
                        break;
                    case nameof(notifyEntry.Message):
                        notifyEntry.Message = new HtmlString(reader.GetString());
                        break;
                }

                return notifyEntry;
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, NotifyEntry value, JsonSerializerOptions options)
        {
            // Serialize the message as it's an IHtmlContent
            using (var stringWriter = new StringWriter(new StringBuilder()))
            {
                value.Message.WriteTo(stringWriter, _htmlEncoder);
            }

            writer.WriteStartObject(nameof(NotifyEntry));
            writer.WriteString(nameof(NotifyEntry.Type), value.Type.ToString());
            writer.WriteString(nameof(NotifyEntry.Message), value.GetMessageAsString(_htmlEncoder));
            writer.WriteEndObject();
        }
    }
}
