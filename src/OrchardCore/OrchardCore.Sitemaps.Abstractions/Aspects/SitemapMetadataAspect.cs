namespace OrchardCore.Sitemaps.Aspects
{
    public class SitemapMetadataAspect
    {
        public string ChangeFrequency { get; set; }
        public int? Priority { get; set; }
        public bool Exclude { get; set; }
    }
}
