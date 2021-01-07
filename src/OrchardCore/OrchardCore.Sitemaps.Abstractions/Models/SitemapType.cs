using System.Collections.Generic;
using Newtonsoft.Json;
using OrchardCore.Data.Documents;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// Abstract to provide a type of sitemap.
    /// </summary>
    public abstract class SitemapType : Document
    {
        private string _path;

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
        public string Path
        {
            get => _path;
            set => _path = value.TrimStart('/');
        }

        /// <summary>
        /// Sitemap unique file name used for caching.
        /// </summary>
        [JsonIgnore]
        public string CacheFileName => Name + "_" + Identifier + Sitemap.PathExtension;

        /// <summary>
        /// Sitemap sources contained by this sitemap.
        /// </summary>
        public List<SitemapSource> SitemapSources { get; set; } = new List<SitemapSource>();
    }
}
