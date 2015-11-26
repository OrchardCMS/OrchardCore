using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement
{
    public class ContentPart : ContentElement, IContent
    {
        public ContentPart() : base()
        {
            Data = new JObject();
        }

        [JsonIgnore]
        public ContentItem ContentItem { get; set; }

        /// <summary>
        /// The ContentItem's identifier.
        /// </summary>
        [JsonIgnore]
        public int ContentItemId => ContentItem.ContentItemId;

        [JsonIgnore()]
        internal override JObject Data { get; set; }

        /// <summary>
        /// The <see cref="ContentTypePartDefinition"/> of the part.
        /// </summary>
        [JsonIgnore]
        public ContentTypePartDefinition TypePartDefinition { get; set; }
    }
}