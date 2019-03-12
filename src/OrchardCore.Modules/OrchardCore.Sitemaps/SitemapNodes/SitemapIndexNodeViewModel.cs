using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Sitemaps.SitemapNodes
{
    public class SitemapIndexNodeViewModel
    {
        [Required]
        public string LinkText { get; set; }
        public string IconClass { get; set; }
    }
}
