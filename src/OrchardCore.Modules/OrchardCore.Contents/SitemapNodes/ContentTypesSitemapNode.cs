using OrchardCore.AdminMenu.Models;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Contents.SitemapNodes
{
    public class ContentTypesSitemapNode : SitemapNode
    {
        public bool ShowAll { get; set; }
        public string IconClass { get; set; }
        public ContentTypeSitemapEntry[] ContentTypes { get; set; } = new ContentTypeSitemapEntry[] { };
    }

    public class ContentTypeSitemapEntry
    {
        public string ContentTypeId { get; set; }
        public string IconClass { get; set; }
    }
}
