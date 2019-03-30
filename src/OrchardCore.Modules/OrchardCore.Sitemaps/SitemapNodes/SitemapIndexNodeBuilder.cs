using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using System.Xml.Linq;

namespace OrchardCore.Sitemaps.SitemapNodes
{
    public class SitemapIndexNodeBuilder : SitemapNodeBuilderBase<SitemapIndexNode>
    {
        public SitemapIndexNodeBuilder()
        {
        }

        public override async Task<XDocument> BuildNodeAsync(SitemapIndexNode sitemapNode, SitemapBuilderContext context)
        {
            //this needs to recurse ChildNodes, but only to one level. Nothing that can site under an index can have multi-levels
            XNamespace defaultNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var root = new XElement(defaultNamespace + "sitemapindex");
            foreach (var node in sitemapNode.ChildNodes)
            {
                var sitemap = new XElement(defaultNamespace + "sitemap");
                var loc = new XElement(defaultNamespace + "loc");
                loc.Add(context.Url.GetBaseUrl() + sitemapNode.SitemapSet.RootPath + node.Path);
                sitemap.Add(loc);

                var lastModDate = await context.Builder.ProvideLastModifiedDateAsync(node, context);
                var lastMod = new XElement(defaultNamespace + "lastmod");
                lastMod.Add(lastModDate.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:sszzz"));
                sitemap.Add(lastMod);

                root.Add(sitemap);
            }
            var document = new XDocument(root);
            return new XDocument(document);
        }
    }
}
