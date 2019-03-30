using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Inherit to provide a sitemap
    /// </summary>
    /// <typeparam name="TSitemapNode"></typeparam>
    public abstract class SitemapNodeBuilderBase<TSitemapNode> : ISitemapNodeBuilder where TSitemapNode : SitemapNode
    {
        async Task ISitemapNodeBuilder.BuildAsync(SitemapNode sitemapNode, SitemapBuilderContext context)
        {
            var sitemap = sitemapNode as TSitemapNode;

            if (sitemap == null)
            {
                return;
            }

            await BuildNodeAsync(sitemap, context);
        }

        public abstract Task BuildNodeAsync(TSitemapNode sitemapNode, SitemapBuilderContext context);
    }
}
