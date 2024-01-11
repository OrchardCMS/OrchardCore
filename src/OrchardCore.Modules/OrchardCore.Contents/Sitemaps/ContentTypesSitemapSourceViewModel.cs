using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapSourceViewModel
    {
        public bool IndexAll { get; set; }

        public bool LimitItems { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

        public int Priority { get; set; } = 5;

        public ContentTypeSitemapEntryViewModel[] ContentTypes { get; set; } = Array.Empty<ContentTypeSitemapEntryViewModel>();

        public ContentTypeLimitedSitemapEntryViewModel[] LimitedContentTypes { get; set; } = Array.Empty<ContentTypeLimitedSitemapEntryViewModel>();

        public string LimitedContentType { get; set; }

        [BindNever]
        public SitemapSource SitemapSource { get; set; }
    }

    public class ContentTypeSitemapEntryViewModel
    {
        public bool IsChecked { get; set; }
        public string ContentTypeDisplayName { get; set; }
        public string ContentTypeName { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public int Priority { get; set; }
    }

    public class ContentTypeLimitedSitemapEntryViewModel
    {
        public string ContentTypeDisplayName { get; set; }
        public string ContentTypeName { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; } = ChangeFrequency.Daily;
        public int Priority { get; set; } = 5;
        public int Skip { get; set; }
        public int Take { get; set; } = 50_000;
    }
}
