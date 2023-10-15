using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Represents a content item version.
    /// </summary>
    [JsonConverter(typeof(ContentItemConverter))]
    public class ContentItem : ContentElement
    {
        public ContentItem() : base()
        {
            ContentItem = this;
        }

        /// <summary>
        /// The primary key in the database.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The logical identifier of the content item across versions.
        /// </summary>
        public string ContentItemId { get; set; }

        /// <summary>
        /// The logical identifier of the versioned content item.
        /// </summary>
        public string ContentItemVersionId { get; set; }

        /// <summary>
        /// The content type of the content item.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Whether the version is published or not.
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Whether the version is the latest version of the content item.
        /// </summary>
        public bool Latest { get; set; }

        /// <summary>
        /// When the content item version has been updated.
        /// </summary>
        public DateTime? ModifiedUtc { get; set; }

        /// <summary>
        /// When the content item has been published.
        /// </summary>
        public DateTime? PublishedUtc { get; set; }

        /// <summary>
        /// When the content item has been created or first published.
        /// </summary>
        public DateTime? CreatedUtc { get; set; }

        /// <summary>
        /// The user id of the user who first created this content item version.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The name of the user who last modified this content item version.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The text representing this content item.
        /// </summary>
        public string DisplayText { get; set; }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(DisplayText) ? $"{ContentType} ({ContentItemId})" : DisplayText;
        }

        /// <summary>
        /// Checks if the provided argument is already a content item, otherwise tries to parse it as JSON.
        /// </summary>
        public static ContentItem Parse(object item)
        {
            ContentItem result;
            switch (item)
            {
                case ContentItem contentItem:
                    return contentItem;
                case JsonObject jsonObject:
                    result = jsonObject.Deserialize<ContentItem>();
                    break;
                case string json when json.TrimStart().StartsWith('{'):
                    result = JsonSerializer.Deserialize<ContentItem>(json);
                    break;
                default:
                    return null;
            }

            // If input is JSON which doesn't represent a ContentItem, then the ContentItem is still created but with
            // some null properties.
            return result?.ContentItem == null ? null : result;
        }
    }
}
