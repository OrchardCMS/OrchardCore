using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public class SitemapBuilder : ISitemapBuilder
    {
        private readonly IEnumerable<ISitemapNodeBuilder> _nodeBuilders;

        public SitemapBuilder(IEnumerable<ISitemapNodeBuilder> nodeBuilders)
        {
            _nodeBuilders = nodeBuilders;
        }

        public async Task<XDocument> BuildAsync(SitemapNode sitemapNode, SitemapBuilderContext context)
        {
            foreach (var nodeBuilder in _nodeBuilders)
            {
                await nodeBuilder.BuildAsync(sitemapNode, context);
            }
            return context.Result;
        }
    }
}
