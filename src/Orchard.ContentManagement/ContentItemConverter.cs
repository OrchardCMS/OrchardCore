using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Orchard.ContentManagement
{
    public class ContentItemConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ContentItem contentItem = (ContentItem)value;
            var o = new JObject();

            // Write all well-known properties
            o.Add(new JProperty(nameof(ContentItem.Id), contentItem.Id));
            o.Add(new JProperty(nameof(ContentItem.ContentItemId), contentItem.ContentItemId));
            o.Add(new JProperty(nameof(ContentItem.ContentType), contentItem.ContentType));
            o.Add(new JProperty(nameof(ContentItem.Latest), contentItem.Latest));
            o.Add(new JProperty(nameof(ContentItem.Number), contentItem.Number));
            o.Add(new JProperty(nameof(ContentItem.Published), contentItem.Published));

            // Write all custom content properties
            o.Merge(contentItem.Data);

            o.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject o = JObject.Load(reader);
            var contentItem = new ContentItem();

            foreach (var p in o.Properties())
            {
                if (p.Name == nameof(ContentItem.Id))
                {
                    contentItem.Id = (int)p.Value;
                }
                else if (p.Name == nameof(ContentItem.ContentItemId))
                {
                    contentItem.ContentItemId = (int)p.Value;
                }
                else if (p.Name == nameof(ContentItem.ContentType))
                {
                    contentItem.ContentType = (string)p.Value;
                }
                else if (p.Name == nameof(ContentItem.Latest))
                {
                    contentItem.Latest = (bool)p.Value;
                }
                else if (p.Name == nameof(ContentItem.Number))
                {
                    contentItem.Number = (int)p.Value;
                }
                else if (p.Name == nameof(ContentItem.Published))
                {
                    contentItem.Published = (bool)p.Value;
                }
                else
                {
                    contentItem.Data.Add(p);
                }
            }
            

            return contentItem;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ContentItem);
        }
    }
   
}
