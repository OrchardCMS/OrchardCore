using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
        }

        /// <summary>
        /// The unique identifier of the current version.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The content item id of this version.
        /// </summary>
        ContentItem IContent.ContentItem => this;

        public int ContentItemId { get; set; }

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
    }
}