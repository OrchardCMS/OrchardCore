using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.SitemapNodes
{
    public class SitemapIndexNode : SitemapNode
    {
        [Required]
        public string LinkText { get; set; }
        public string IconClass { get; set; }
    }
}
