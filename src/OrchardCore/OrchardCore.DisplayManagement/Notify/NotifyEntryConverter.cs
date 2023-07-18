using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.DisplayManagement.Notify
{
    public class NotifyEntryConverter : JsonConverter
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            NotifyType type;

            var notifyEntry = new NotifyEntry
            {
                Message = new HtmlString(jo.Value<string>("Message")),
            };

            if (Enum.TryParse(jo.Value<string>("Type"), out type))
            {
                notifyEntry.Type = type;
            }

            return notifyEntry;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var notifyEntry = value as NotifyEntry;
            if (notifyEntry == null)
            {
                return;
            }

            var o = new JObject();

            // Serialize the message as it's an IHtmlContent
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
            {
                notifyEntry.Message.WriteTo(stringWriter, _htmlEncoder);
            }

            // Write all well-known properties
            o.Add(new JProperty(nameof(NotifyEntry.Type), notifyEntry.Type.ToString()));
            o.Add(new JProperty(nameof(NotifyEntry.Message), notifyEntry.GetMessageAsString(_htmlEncoder)));

            o.WriteTo(writer);
        }
    }
}
