namespace OrchardCore.Sitemaps.Models
{
    // 'MessagePack' can't serialize an abstract class.
    public /*abstract*/ class SitemapSource
    {
        public string Id { get; set; }
    }
}
