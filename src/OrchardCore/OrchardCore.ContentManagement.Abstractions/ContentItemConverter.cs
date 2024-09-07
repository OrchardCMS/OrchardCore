using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.ContentManagement;

public class ContentItemConverter : JsonConverter<ContentItem>
{
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
        o.Merge(value.Data);

        o.WriteTo(writer);
    }

    public override ContentItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var contentItem = new ContentItem();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var propertyName = reader.GetString();

            reader.Read();

            switch (propertyName)
            {
                case nameof(ContentItem.ContentItemId):
                    contentItem.ContentItemId = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    break;
                case nameof(ContentItem.ContentItemVersionId):
                    contentItem.ContentItemVersionId = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    break;
                case nameof(ContentItem.ContentType):
                    contentItem.ContentType = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    break;
                case nameof(ContentItem.DisplayText):
                    contentItem.DisplayText = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    break;
                case nameof(ContentItem.Latest):
                    contentItem.Latest = (reader.TokenType == JsonTokenType.True ||
                        reader.TokenType == JsonTokenType.False) && reader.GetBoolean();
                    break;
                case nameof(ContentItem.Published):
                    contentItem.Published = (reader.TokenType == JsonTokenType.True ||
                        reader.TokenType == JsonTokenType.False) && reader.GetBoolean();
                    break;
                case nameof(ContentItem.PublishedUtc):
                    contentItem.PublishedUtc = reader.TokenType != JsonTokenType.Null &&
                        reader.TryGetDateTime(out var date) ? date : null;
                    break;
                case nameof(ContentItem.ModifiedUtc):
                    contentItem.ModifiedUtc = reader.TokenType != JsonTokenType.Null &&
                        reader.TryGetDateTime(out date) ? date : null;
                    break;
                case nameof(ContentItem.CreatedUtc):
                    contentItem.CreatedUtc = reader.TokenType != JsonTokenType.Null &&
                        reader.TryGetDateTime(out date) ? date : null;
                    break;
                case nameof(ContentItem.Author):
                    contentItem.Author = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    break;
                case nameof(ContentItem.Owner):
                    contentItem.Owner = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    break;
                default:
                    if (reader.TokenType == JsonTokenType.StartObject ||
                        reader.TokenType == JsonTokenType.StartArray)
                    {
                        var property = JNode.Load(ref reader);
                        contentItem.Data[propertyName] = property;
                    }

                    break;
            }
        }

        return contentItem;
    }
}
