using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Sitemaps.SitemapNodes
{
    public class SitemapIndexNodeViewModel
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
