using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Inherit to provide a sitemap builder.
    /// </summary>
    /// <typeparam name="TSitemap"></typeparam>
    public abstract class SitemapBuilderBase<TSitemap> : ISitemapBuilder where TSitemap : Sitemap
    {
        async Task<XDocument> ISitemapBuilder.BuildAsync(Sitemap sitemap, SitemapBuilderContext context)
        {
            var tSitemap = sitemap as TSitemap;
            if (tSitemap == null  || !tSitemap.Enabled)
            {
                return null;
            }

            return await BuildSitemapsAsync(tSitemap, context);
        }

        public abstract Task<XDocument> BuildSitemapsAsync(TSitemap sitemap, SitemapBuilderContext context);

        async Task<DateTime?> ISitemapBuilder.GetLastModifiedDateAsync(Sitemap sitemap, SitemapBuilderContext context)
        {
            var tSitemap = sitemap as TSitemap;

            if (tSitemap == null || !tSitemap.Enabled)
            {
                return null;
            }
            return await GetLastModifiedDateAsync(tSitemap, context);
        }

        // Override to implement, as required.
        public virtual Task<DateTime?> GetLastModifiedDateAsync(TSitemap sitemap, SitemapBuilderContext context)
        {
            return Task.FromResult<DateTime?>(null);
        }
    }
}
