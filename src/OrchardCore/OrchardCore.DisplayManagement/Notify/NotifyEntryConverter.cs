using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
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

        public override bool CanConvert(Type objectType)
        {
            return typeof(NotifyEntry).IsAssignableFrom(objectType);
        }

        public override NotifyEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jo = JObject.Load(ref reader);

            var notifyEntry = new NotifyEntry
            {
                Message = new HtmlString(jo.Value<string>("Message")),
            };

            if (Enum.TryParse<NotifyType>(jo.Value<string>("Type"), out var type))
            {
                notifyEntry.Type = type;
            }

            return notifyEntry;
        }

        public override void Write(Utf8JsonWriter writer, NotifyEntry value, JsonSerializerOptions options)
        {
            var notifyEntry = value;
            if (notifyEntry == null)
            {
                return;
            }

            var o = new JsonObject();

            // Serialize the message as it's an IHtmlContent
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                notifyEntry.Message.WriteTo(stringWriter, _htmlEncoder);
            }

            // Write all well-known properties
            o.Add(nameof(NotifyEntry.Type), notifyEntry.Type.ToString());
            o.Add(nameof(NotifyEntry.Message), notifyEntry.GetMessageAsString(_htmlEncoder));

            o.WriteTo(writer);
        }
    }
}
