using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// SitemapPart is an optional part, which allows individual content items to be excluded from sitemaps.
    /// </summary>
    public class SitemapPart : ContentPart
    {
        public bool OverrideSitemapConfig { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; } = ChangeFrequency.Daily;
        public int Priority { get; set; } = 5;
        public bool Exclude { get; set; }
    }
}
