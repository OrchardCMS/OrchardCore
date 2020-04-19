using System.Collections.Generic;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// Abstract to provide a type of sitemap.
    /// </summary>
    // 'MessagePack' can't serialize an abstract class.
    public /*abstract*/ class SitemapType
    {
        /// <summary>
        /// Sitemap id.
        /// </summary>
        public string SitemapId { get; set; }

        /// <summary>
        /// Name of sitemap.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// When false sitemap will not be included in routing.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Sitemap path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Sitemap sources contained by this sitemap.
        /// </summary>
        public List<SitemapSource> SitemapSources { get; set; } = new List<SitemapSource>();
    }
}
