using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement
{
    public class ContentItemConverter : JsonConverter
    {
        private readonly JsonLoadSettings _jsonLoadSettings = new()
        {
            LineInfoHandling = LineInfoHandling.Ignore, // Defaults to loading which allocates quite a lot.
        };

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var contentItem = (ContentItem)value;
            var o = new JObject
            {
                // Write all well-known properties.
                new JProperty(nameof(ContentItem.ContentItemId), contentItem.ContentItemId),
                new JProperty(nameof(ContentItem.ContentItemVersionId), contentItem.ContentItemVersionId),
                new JProperty(nameof(ContentItem.ContentType), contentItem.ContentType),
                new JProperty(nameof(ContentItem.DisplayText), contentItem.DisplayText),
                new JProperty(nameof(ContentItem.Latest), contentItem.Latest),
                new JProperty(nameof(ContentItem.Published), contentItem.Published),
                new JProperty(nameof(ContentItem.ModifiedUtc), contentItem.ModifiedUtc),
                new JProperty(nameof(ContentItem.PublishedUtc), contentItem.PublishedUtc),
                new JProperty(nameof(ContentItem.CreatedUtc), contentItem.CreatedUtc),
                new JProperty(nameof(ContentItem.Owner), contentItem.Owner),
                new JProperty(nameof(ContentItem.Author), contentItem.Author),
            };

            // Write all custom content properties.
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
                        var customProperty = JProperty.Load(reader, _jsonLoadSettings);
                        contentItem.Data.Add(customProperty);

                        // Skip reading a token as JProperty.Load already did the next one.
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
