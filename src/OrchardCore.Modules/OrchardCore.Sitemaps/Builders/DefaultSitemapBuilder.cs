using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public class DefaultSitemapBuilder : ISitemapBuilder
    {
        private readonly IEnumerable<ISitemapNodeBuilder> _nodeBuilders;

        public DefaultSitemapBuilder(IEnumerable<ISitemapNodeBuilder> nodeBuilders)
        {
            _nodeBuilders = nodeBuilders;
        }

        public async Task<XDocument> BuildAsync(SitemapNode sitemapNode, SitemapBuilderContext context)
        {
            foreach (var nodeBuilder in _nodeBuilders)
            {
                var result = await nodeBuilder.BuildAsync(sitemapNode, context);
                if (result != null)
                    return result;
            }
            return null;
        }

        public async Task<DateTime?> GetLastModifiedDateAsync(SitemapNode sitemapNode, SitemapBuilderContext context)
        {
            foreach (var nodeBuilder in _nodeBuilders)
            {
                var result = await nodeBuilder.GetLastModifiedDateAsync(sitemapNode, context);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
