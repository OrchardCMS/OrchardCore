using System.ComponentModel.DataAnnotations;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Contents.SitemapNodes
{
    public class ContentTypesSitemapNode : SitemapNode
    {
        /// <summary>
        /// Description of the sitemap
        /// </summary>
        [Required]
        public string Description { get; set; }
        public bool IndexAll { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public float Priority { get; set; }
        public ContentTypeSitemapEntry[] ContentTypes { get; set; } = new ContentTypeSitemapEntry[] { };
    }

    public class ContentTypeSitemapEntry
    {
        //is this actualy ContentTypeName ? if so rename
        public string ContentTypeId { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        //TODO think this can change to priority now
        public float IndexPriority { get; set; }
    }
}
