using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapSetEditViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
