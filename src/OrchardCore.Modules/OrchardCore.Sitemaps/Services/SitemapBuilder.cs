using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapBuilder : ISitemapBuilder
    {
        private readonly IEnumerable<ISitemapNodeNavigationBuilder> _nodeBuilders;

        public SitemapBuilder(IEnumerable<ISitemapNodeNavigationBuilder> nodeBuilders)
        {
            _nodeBuilders = nodeBuilders;
        }

        public async Task<XDocument> BuildAsync(SitemapNode sitemapNode, SitemapBuilderContext sitemapContext)
        {
            //    var rss = new XElement("rss");
            //    rss.SetAttributeValue("version", "2.0");

            //    var channel = new XElement("channel");
            //    context.Response.Element = channel;
            //    rss.Add(channel);

            //    await populate();

            //    return new XDocument(rss);

            foreach (var nodeBuilder in _nodeBuilders)
            {
                await nodeBuilder.BuildSitemapAsync(sitemapNode, _nodeBuilders);
            }
            return sitemapContext.Result;

        }
    }
}
