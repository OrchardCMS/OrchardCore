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

    }
}
