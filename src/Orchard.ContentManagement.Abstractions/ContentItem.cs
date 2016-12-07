using Newtonsoft.Json;
using System;

namespace Orchard.ContentManagement
{
    /// <summary>
    /// Represents a content item version.
    /// </summary>
    [JsonConverter(typeof(ContentItemConverter))]
    public class ContentItem : ContentElement, IContent
    {
        public ContentItem() : base()
        {
            ContentItem = this;
        }

        /// <summary>
        /// The primary key in the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The logical identifier of the content item.
        /// </summary>
        public string ContentItemId { get; set; }

        /// <summary>
        /// The content type of the content item.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The number of the version.
        /// </summary>
        public int Number { get; set; }

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
        public DateTimeOffset? ModifiedUtc { get; set; }

        /// <summary>
        /// When the content item has been published.
        /// </summary>
        public DateTimeOffset? PublishedUtc { get; set; }

        /// <summary>
        /// When the content item has been created or first published.
        /// </summary>
        public DateTimeOffset? CreatedUtc { get; set; }

        /// <summary>
        /// The name of the user who last modified this content item version.
        /// </summary>
        public string ModifiedBy { get; set; }
    }
}