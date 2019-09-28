using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapSetEditViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        //TODO BasePath and PathString
        public string RootPath { get; set; }
    }
}
