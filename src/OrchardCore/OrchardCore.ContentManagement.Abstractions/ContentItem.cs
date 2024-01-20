using System;
using Newtonsoft.Json;

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
    }
}
