using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Inherit to provide a sitemap type builder.
    /// </summary>
    public abstract class SitemapTypeBuilderBase<TSitemapType> : ISitemapTypeBuilder where TSitemapType : SitemapType
    {
        public async Task BuildAsync(SitemapType sitemap, SitemapBuilderContext context)
        {
            if (sitemap is TSitemapType tSitemap)
            {
                await BuildSitemapTypeAsync(tSitemap, context);
            }
        }

        public abstract Task BuildSitemapTypeAsync(TSitemapType sitemap, SitemapBuilderContext context);
    }
}
