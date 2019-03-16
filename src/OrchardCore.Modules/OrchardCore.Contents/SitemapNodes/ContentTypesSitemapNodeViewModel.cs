using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Contents.SitemapNodes
{
    public class ContentTypesSitemapNodeViewModel
    {
        public bool IndexAll { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public float IndexPriority { get; set; }
        public ContentTypeSitemapEntryViewModel[] ContentTypes { get; set; } = new ContentTypeSitemapEntryViewModel[] { };
    }

    public class ContentTypeSitemapEntryViewModel
    {
        public bool IsChecked { get; set; }
        public string ContentTypeId { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; }
        public float IndexPriority { get; set; }
    }
}
