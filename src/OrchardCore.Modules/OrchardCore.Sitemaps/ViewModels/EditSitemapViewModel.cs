using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class EditSitemapViewModel
    {
        public string SitemapId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Path { get; set; }

        public bool Enabled { get; set; }
    }
}
