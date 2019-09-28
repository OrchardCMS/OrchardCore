using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Inherit to provide a sitemap builder.
    /// </summary>
    /// <typeparam name="TSitemapNode"></typeparam>
    public abstract class SitemapNodeBuilderBase<TSitemapNode> : ISitemapNodeBuilder where TSitemapNode : SitemapNode
    {
        async Task<XDocument> ISitemapNodeBuilder.BuildAsync(SitemapNode sitemapNode, SitemapBuilderContext context)
        {
            var node = sitemapNode as TSitemapNode;
            // Be exact type to handle inheritance.
            if (node == null || node.GetType() != typeof(TSitemapNode) || !node.Enabled)
            {
                return null;
            }

            return await BuildNodeAsync(node, context);
        }

        public abstract Task<XDocument> BuildNodeAsync(TSitemapNode sitemapNode, SitemapBuilderContext context);

        async Task<DateTime?> ISitemapNodeBuilder.GetLastModifiedDateAsync(SitemapNode sitemapNode, SitemapBuilderContext context)
        {
            var node = sitemapNode as TSitemapNode;

            if (node == null || !node.Enabled)
            {
                return null;
            }
            return await GetNodeLastModifiedDateAsync(node, context);
        }

        // Override to implement
        public virtual Task<DateTime?> GetNodeLastModifiedDateAsync(TSitemapNode sitemapNode, SitemapBuilderContext context)
        {
            return Task.FromResult<DateTime?>(null);
        }
    }
}
