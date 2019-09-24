using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapSetCreateViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string RootPath { get; set; }
    }
}
