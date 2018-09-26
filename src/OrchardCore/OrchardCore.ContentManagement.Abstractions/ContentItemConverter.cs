using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement
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
            o.Add(new JProperty(nameof(ContentItem.ContentItemVersionId), contentItem.ContentItemVersionId));
            o.Add(new JProperty(nameof(ContentItem.ContentType), contentItem.ContentType));
            o.Add(new JProperty(nameof(ContentItem.DisplayText), contentItem.DisplayText));
            o.Add(new JProperty(nameof(ContentItem.Latest), contentItem.Latest));
            o.Add(new JProperty(nameof(ContentItem.Published), contentItem.Published));
            o.Add(new JProperty(nameof(ContentItem.ModifiedUtc), contentItem.ModifiedUtc));
            o.Add(new JProperty(nameof(ContentItem.PublishedUtc), contentItem.PublishedUtc));
            o.Add(new JProperty(nameof(ContentItem.CreatedUtc), contentItem.CreatedUtc));
            o.Add(new JProperty(nameof(ContentItem.Owner), contentItem.Owner));
            o.Add(new JProperty(nameof(ContentItem.Author), contentItem.Author));

            // Write all custom content properties
            o.Merge(contentItem.Data);

            o.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var contentItem = new ContentItem();
            var skip = false;

            while (skip || reader.Read())
            {
                skip = false;

                if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                var propertyName = (string)reader.Value;

                switch (propertyName)
                {
                    case nameof(ContentItem.Id) : 
                        contentItem.Id = reader.ReadAsInt32() ?? 0;
                        break;
                    case nameof(ContentItem.ContentItemId):
                        contentItem.ContentItemId = reader.ReadAsString();
                        break;
                    case nameof(ContentItem.ContentItemVersionId):
                        contentItem.ContentItemVersionId = reader.ReadAsString();
                        break;
                    case nameof(ContentItem.ContentType):
                        contentItem.ContentType = reader.ReadAsString();
                        break;
                    case nameof(ContentItem.DisplayText):
                        contentItem.DisplayText = reader.ReadAsString();
                        break;
                    case nameof(ContentItem.Latest):
                        contentItem.Latest = reader.ReadAsBoolean() ?? false;
                        break;
                    case nameof(ContentItem.Published):
                        contentItem.Published = reader.ReadAsBoolean() ?? false;
                        break;
                    case nameof(ContentItem.PublishedUtc):
                        contentItem.PublishedUtc = reader.ReadAsDateTime();
                        break;
                    case nameof(ContentItem.ModifiedUtc):
                        contentItem.ModifiedUtc = reader.ReadAsDateTime();
                        break;
                    case nameof(ContentItem.CreatedUtc):
                        contentItem.CreatedUtc = reader.ReadAsDateTime();
                        break;
                    case nameof(ContentItem.Author):
                        contentItem.Author = reader.ReadAsString();
                        break;
                    case nameof(ContentItem.Owner):
                        contentItem.Owner = reader.ReadAsString();
                        break;
                    default:
                        var customProperty = JProperty.Load(reader);
                        contentItem.Data.Add(customProperty);

                        // Skip reading a token as JPproperty.Load already did the next one
                        skip = true;
                        break;
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
