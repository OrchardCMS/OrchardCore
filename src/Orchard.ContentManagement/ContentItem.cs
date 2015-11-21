using System;
using System.Collections.Generic;

namespace Orchard.ContentManagement
{
    /// <summary>
    /// Represents a content item version.
    /// </summary>
    public class ContentItem : IContent
    {
        public ContentItem()
        {
            Parts = new Dictionary<string, ContentPart>();
        }

        /// <summary>
        /// The unique identifier of the current version.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The content item id of this version.
        /// </summary>
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

        /// <summary>
        /// The list of parts for this version.
        /// </summary>
        public Dictionary<string, ContentPart> Parts { get; set; }

        ContentItem IContent.ContentItem => this;

        public bool Has(Type partType)
        {
            return Has(partType.Name);
        }

        public bool Has(string partName)
        {
            return Parts.ContainsKey(partName);
        }

        public IContent Get(Type partType)
        {
            return Get(partType.Name);
        }

        public IContent Get(string partName)
        {
            return Parts[partName];
        }

        public void Weld(ContentPart part)
        {
            Parts.Add(part.GetType().Name, part);
        }
    }
}