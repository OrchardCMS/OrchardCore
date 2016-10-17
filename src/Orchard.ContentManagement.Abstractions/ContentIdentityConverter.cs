using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Orchard.ContentManagement
{
    public class ContentIdentityConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ContentIdentity contentIdentity = (ContentIdentity)value;
            var o = new JObject();

            foreach (var name in contentIdentity.Names)
            {
                o.Add(new JProperty(name, contentIdentity.Get(name)));
            }

            o.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject o = JObject.Load(reader);
            var contentIdentity = new ContentIdentity();

            foreach (var p in o.Properties())
            {
                if (p.Value != null && p.Value.Type == JTokenType.String)
                {
                    contentIdentity.Add(p.Name, p.Value.ToString());
                }
            }
            
            return contentIdentity;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ContentIdentity);
        }
    }
   
}
