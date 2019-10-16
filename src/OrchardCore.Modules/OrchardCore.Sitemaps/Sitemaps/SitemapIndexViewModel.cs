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

        public ContainableSitemapEntryViewModel[] ContainableSitemaps { get; set; } = new ContainableSitemapEntryViewModel[] { };

        [BindNever]
        public Sitemap Sitemap { get; set; }

    }

    public class ContainableSitemapEntryViewModel
    {
        public bool IsChecked { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
