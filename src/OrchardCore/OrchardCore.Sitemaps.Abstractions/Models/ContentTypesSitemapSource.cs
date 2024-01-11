using System;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// A sitemap source for managing content by content type.
    /// </summary>
    public class ContentTypesSitemapSource : SitemapSource
    {
        /// <summary>
        /// Index all content types.
        /// </summary>
        public bool IndexAll { get; set; } = true;

        /// <summary>
        /// Limit quantity of content items in sitemap.
        /// </summary>
        public bool LimitItems { get; set; }

        /// <summary>
        /// Change frequency to apply to sitemap entries.
        /// </summary>
        public ChangeFrequency ChangeFrequency { get; set; }

        // Handle as int, and convert to float, when building, to support localization.
        public int Priority { get; set; } = 5;

        /// <summary>
        /// When not indexing all content types this contains the list of content types to index.
        /// </summary>
        public ContentTypeSitemapEntry[] ContentTypes { get; set; } = Array.Empty<ContentTypeSitemapEntry>();

        /// <summary>
        /// When limiting content items, only one content type can be specified.
        /// </summary>
        public LimitedContentTypeSitemapEntry LimitedContentType { get; set; } = new LimitedContentTypeSitemapEntry();
    }

    public class ContentTypeSitemapEntry
    {
        public string ContentTypeName { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public int Priority { get; set; }
    }

    public class LimitedContentTypeSitemapEntry : ContentTypeSitemapEntry
    {
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
