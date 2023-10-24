using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.ContentManagement
{
    public class ContentItemConverter : JsonConverter<ContentItem>
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(ContentItem);

        public override void Write(Utf8JsonWriter writer, ContentItem value, JsonSerializerOptions options)
        {
            var o = new JsonObject()
            {
                // Write all well-known properties.
                [nameof(ContentItem.ContentItemId)] = value.ContentItemId,
                [nameof(ContentItem.ContentItemVersionId)] = value.ContentItemVersionId,
                [nameof(ContentItem.ContentType)] = value.ContentType,
                [nameof(ContentItem.DisplayText)] = value.DisplayText,
                [nameof(ContentItem.Latest)] = value.Latest,
                [nameof(ContentItem.Published)] = value.Published,
                [nameof(ContentItem.ModifiedUtc)] = value.ModifiedUtc,
                [nameof(ContentItem.PublishedUtc)] = value.PublishedUtc,
                [nameof(ContentItem.CreatedUtc)] = value.CreatedUtc,
                [nameof(ContentItem.Owner)] = value.Owner,
                [nameof(ContentItem.Author)] = value.Author,
            };

            // Write all custom content properties.
            o.Merge(value.Data, options);

            o.WriteTo(writer);
        }

        public override ContentItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var contentItem = new ContentItem();
            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case nameof(ContentItem.ContentItemId):
                        reader.Read();
                        contentItem.ContentItemId = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                        break;
                    case nameof(ContentItem.ContentItemVersionId):
                        reader.Read();
                        contentItem.ContentItemVersionId = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                        break;
                    case nameof(ContentItem.ContentType):
                        reader.Read();
                        contentItem.ContentType = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                        break;
                    case nameof(ContentItem.DisplayText):
                        reader.Read();
                        contentItem.DisplayText = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                        break;
                    case nameof(ContentItem.Latest):
                        reader.Read();
                        contentItem.Latest = reader.TokenType == JsonTokenType.True ||
                            reader.TokenType == JsonTokenType.False ? reader.GetBoolean() : false;
                        break;
                    case nameof(ContentItem.Published):
                        reader.Read();
                        contentItem.Published = reader.TokenType == JsonTokenType.True ||
                            reader.TokenType == JsonTokenType.False ? reader.GetBoolean() : false;
                        break;
                    case nameof(ContentItem.PublishedUtc):
                        reader.Read();
                        contentItem.PublishedUtc = reader.TryGetDateTime(out var date) ? date : null;
                        break;
                    case nameof(ContentItem.ModifiedUtc):
                        reader.Read();
                        contentItem.ModifiedUtc = reader.TryGetDateTime(out date) ? date : null;
                        break;
                    case nameof(ContentItem.CreatedUtc):
                        reader.Read();
                        contentItem.CreatedUtc = reader.TryGetDateTime(out date) ? date : null;
                        break;
                    case nameof(ContentItem.Author):
                        reader.Read();
                        contentItem.Author = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                        break;
                    case nameof(ContentItem.Owner):
                        reader.Read();
                        contentItem.Owner = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                        break;
                    default:
                        var jsonElement = JsonElement.ParseValue(ref reader);
                        contentItem.Data.MergeItem(jsonElement, options);
                        break;
                }
            }

            return contentItem;
        }
    }
}
