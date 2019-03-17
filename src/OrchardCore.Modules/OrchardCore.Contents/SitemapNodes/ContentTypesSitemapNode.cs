using System.ComponentModel.DataAnnotations;
using OrchardCore.AdminMenu.Models;
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
        public string ContentTypeId { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public float IndexPriority { get; set; }
    }
}
