using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class EditSitemapIndexSourceViewModel
    {
        [Required]
        public string SitemapId { get; set; }

        [Required]
        public string Path { get; set; }

        public bool Enabled { get; set; }

        public ContainableSitemapEntryViewModel[] ContainableSitemaps { get; set; } = new ContainableSitemapEntryViewModel[] { };

        [BindNever]
        public bool IsNew { get; set; }

        [BindNever]
        public SitemapIndexSource SitemapIndexSource { get; set; }
    }

    public class ContainableSitemapEntryViewModel
    {
        public bool IsChecked { get; set; }
        public string SitemapId { get; set; }
        public string Name { get; set; }
    }
}
