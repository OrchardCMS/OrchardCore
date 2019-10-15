using OrchardCore.ContentManagement;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapPart : ContentPart
    {
        public bool OverrideSitemapConfig { get; set; }
        public ChangeFrequency ChangeFrequency { get; set; } = ChangeFrequency.Daily;
        public float Priority { get; set; } = 0.5f;
        public bool Exclude { get; set; }
    }
}
