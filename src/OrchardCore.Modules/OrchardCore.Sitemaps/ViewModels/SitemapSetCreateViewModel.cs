using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class SitemapSetCreateViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
