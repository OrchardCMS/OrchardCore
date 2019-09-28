using System.ComponentModel.DataAnnotations;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.SitemapNodes
{
    public class SitemapIndexNode : SitemapNode
    {
        /// <summary>
        /// Description of the sitemap index.
        /// </summary>
        [Required]
        public string Description { get; set; }

        public override bool CanBeChildNode => false;

        public override bool CanSupportChildNodes => true;
    }
}
