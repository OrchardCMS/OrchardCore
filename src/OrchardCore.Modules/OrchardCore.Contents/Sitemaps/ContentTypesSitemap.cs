using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemap : Sitemap
    {
        public bool IndexAll { get; set; } = true;
        public ChangeFrequency ChangeFrequency { get; set; }

        // Handle as int, and convert to float, when building, to support localization.
        public int Priority { get; set; } = 5;
        public ContentTypeSitemapEntry[] ContentTypes { get; set; } = new ContentTypeSitemapEntry[] { };
    }

    public class ContentTypeSitemapEntry
    {
        public string ContentTypeName { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public int Priority { get; set; }
        public bool TakeAll { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
