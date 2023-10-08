using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement
{
    public class ContentItemConverter : JsonConverter<ContentItem>
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(ContentItem);

        public override ContentItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var contentItem = new ContentItem();
            var skip = false;
            while (skip || reader.Read())
            {
                skip = false;
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    break;
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(ContentItem.ContentItemId):
                        contentItem.ContentItemId = reader.GetString();
                        break;
                    case nameof(ContentItem.ContentItemVersionId):
                        contentItem.ContentItemVersionId = reader.GetString();
                        break;
                    case nameof(ContentItem.ContentType):
                        contentItem.ContentType = reader.GetString();
                        break;
                    case nameof(ContentItem.DisplayText):
                        contentItem.DisplayText = reader.GetString();
                        break;
                    case nameof(ContentItem.Latest):
                        contentItem.Latest = reader.GetBoolean();
                        break;
                    case nameof(ContentItem.Published):
                        contentItem.Published = reader.GetBoolean();
                        break;
                    case nameof(ContentItem.PublishedUtc):
                        contentItem.PublishedUtc = reader.GetDateTime();
                        break;
                    case nameof(ContentItem.ModifiedUtc):
                        contentItem.ModifiedUtc = reader.GetDateTime();
                        break;
                    case nameof(ContentItem.CreatedUtc):
                        contentItem.CreatedUtc = reader.GetDateTime();
                        break;
                    case nameof(ContentItem.Author):
                        contentItem.Author = reader.GetString();
                        break;
                    case nameof(ContentItem.Owner):
                        contentItem.Owner = reader.GetString();
                        break;
                    default:
                        // var customProperty = JProperty.Load(reader, _jsonLoadSettings);
                        // contentItem.Data.Add(customProperty);

                        // Skip reading a token as JProperty.Load already did the next one.
                        skip = true;
                        break;
                }
            }

            return contentItem;
        }

        public override void Write(Utf8JsonWriter writer, ContentItem value, JsonSerializerOptions options)
        {
            writer.WriteStartObject(nameof(ContentItem));
            writer.WriteString(nameof(ContentItem.ContentItemId), value.ContentItemId);
            writer.WriteString(nameof(ContentItem.ContentItemVersionId), value.ContentItemVersionId);
            writer.WriteString(nameof(ContentItem.ContentType), value.ContentType);
            writer.WriteString(nameof(ContentItem.DisplayText), value.DisplayText);
            writer.WriteBoolean(nameof(ContentItem.Latest), value.Latest);
            writer.WriteBoolean(nameof(ContentItem.Published), value.Published);
            writer.WriteString(nameof(ContentItem.ModifiedUtc), value.ModifiedUtc.ToString());
            writer.WriteString(nameof(ContentItem.PublishedUtc), value.PublishedUtc.ToString());
            writer.WriteString(nameof(ContentItem.CreatedUtc), value.CreatedUtc.ToString());
            writer.WriteString(nameof(ContentItem.Owner), value.Owner);
            writer.WriteString(nameof(ContentItem.Author), value.Author);

            WriteJObject(writer, value.Data);

            writer.WriteEndObject();
        }

        private static void WriteJObject(Utf8JsonWriter writer, JObject data)
        {
            foreach (var item in data)
            {
                if (item.Value != null)
                {
                    switch (item.Value.Type)
                    {
                        case JTokenType.Boolean:
                            writer.WriteBoolean(item.Key, item.Value.Value<bool>());
                            break;
                        case JTokenType.Integer:
                            writer.WriteNumber(item.Key, item.Value.Value<int>());
                            break;
                        case JTokenType.Null:
                            writer.WriteNull(item.Key);
                            break;
                        case JTokenType.Array:
                        case JTokenType.Object:
                        case JTokenType.Bytes:
                        case JTokenType.None:
                        case JTokenType.Constructor:
                        case JTokenType.Property:
                        case JTokenType.Comment:
                        case JTokenType.Undefined:
                        case JTokenType.Raw:
                            break;
                        case JTokenType.Float:
                        case JTokenType.Guid:
                        case JTokenType.Uri:
                        case JTokenType.TimeSpan:
                        case JTokenType.String:
                        case JTokenType.Date:
                            writer.WriteString(item.Key, item.Value.Value<string>());
                            break;
                    }
                }
            }
        }
    }
}
