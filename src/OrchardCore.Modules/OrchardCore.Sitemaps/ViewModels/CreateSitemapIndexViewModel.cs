using System;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class CreateSitemapIndexViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Path { get; set; }

        public bool Enabled { get; set; }

        public ContainableSitemapEntryViewModel[] ContainableSitemaps { get; set; } = Array.Empty<ContainableSitemapEntryViewModel>();

    }

    public class ContainableSitemapEntryViewModel
    {
        public bool IsChecked { get; set; }
        public string SitemapId { get; set; }
        public string Name { get; set; }
    }
}
