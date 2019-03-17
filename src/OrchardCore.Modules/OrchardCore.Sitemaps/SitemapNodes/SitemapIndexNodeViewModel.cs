using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.SitemapNodes
{
    public class SitemapIndexNodeViewModel
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public string Path { get; set; }

        [BindNever]
        public SitemapNode SitemapNode { get; set; }
    }
}
