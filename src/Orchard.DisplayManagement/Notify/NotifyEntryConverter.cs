using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace Orchard.DisplayManagement.Notify
{
    public class NotifyEntryConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return typeof(NotifyEntry).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            NotifyType type;

            var notifyEntry = new NotifyEntry();
            notifyEntry.Message = new HtmlString(jo.Value<string>("Message"));

            if(Enum.TryParse(jo.Value<string>("Type"), out type))
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

            // Write all well-known properties
            o.Add(new JProperty(nameof(NotifyEntry.Type), notifyEntry.Type.ToString()));
            o.Add(new JProperty(nameof(NotifyEntry.Message), notifyEntry.Message.ToString()));

            o.WriteTo(writer);
        }
    }
}