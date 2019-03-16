using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.SitemapNodes
{
    public class SitemapIndexNode : SitemapNode
    {
        /// <summary>
        /// Description of the sitemap
        /// </summary>
        [Required]
        
        public string Description { get; set; }

        /// <summary>
        /// name of the file served, i.e. sitemap-index.xml, or sitemap-content.xml, or sitemap.xml. TODO use for routing
        /// Probably move to base
        /// </summary>
        [Required]
        public string Name { get; set; }
    }
}
