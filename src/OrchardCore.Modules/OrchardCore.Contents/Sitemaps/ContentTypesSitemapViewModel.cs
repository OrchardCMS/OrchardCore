using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapViewModel
    {
        public bool IndexAll { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

        public int Priority { get; set; } = 5;

        [Required]
        public string Name { get; set; }

        public string Path { get; set; }

        public ContentTypeSitemapEntryViewModel[] ContentTypes { get; set; } = new ContentTypeSitemapEntryViewModel[] { };

        [BindNever]
        public Sitemap Sitemap { get; set; }
    }

    public class ContentTypeSitemapEntryViewModel
    {
        public bool IsChecked { get; set; }
        public string ContentTypeDisplayName { get; set; }
        public string ContentTypeName { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public int Priority { get; set; }
        public bool TakeAll { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
