using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Sitemaps
{
    public class SitemapIndexViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Path { get; set; }

        [BindNever]
        public Sitemap SitemapNode { get; set; }

    }
}
