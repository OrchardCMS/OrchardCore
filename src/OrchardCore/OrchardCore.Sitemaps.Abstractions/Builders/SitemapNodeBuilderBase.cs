using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Inherit to provide a sitemap
    /// </summary>
    /// <typeparam name="TSitemapNode"></typeparam>
    public abstract class SitemapNodeBuilderBase<TSitemapNode> : ISitemapNodeBuilder where TSitemapNode : SitemapNode
    {
        async Task<XDocument> ISitemapNodeBuilder.BuildAsync(SitemapNode sitemapNode, SitemapBuilderContext context)
        {
            var node = sitemapNode as TSitemapNode;

            if (node == null)
            {
                return null;
            }

            return await BuildNodeAsync(node, context);
        }

        public abstract Task<XDocument> BuildNodeAsync(TSitemapNode sitemapNode, SitemapBuilderContext context);

        async Task<DateTime?> ISitemapNodeBuilder.ProvideLastModifiedDateAsync(SitemapNode sitemapNode, SitemapBuilderContext context)
        {
            var node = sitemapNode as TSitemapNode;

            if (node == null)
            {
                return null ;
            }
            return await ProvideNodeLastModifiedDateAsync(node, context);
        }

        //override to provide
        public virtual Task<DateTime?> ProvideNodeLastModifiedDateAsync(TSitemapNode sitemapNode, SitemapBuilderContext context)
        {
            return Task.FromResult<DateTime?>(null);
        }
    }
}
