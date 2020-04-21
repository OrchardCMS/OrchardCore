using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public class SitemapTypeBuilder : SitemapTypeBuilderBase<Sitemap>
    {
        private static readonly XNamespace Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        private static readonly XNamespace SchemaInstance = "http://wwww.w3.org/2001/XMLSchema-instance";
        private static readonly XNamespace SchemaLocation = "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd";

        private readonly IEnumerable<ISitemapSourceBuilder> _sitemapSourceBuilders;

        public SitemapTypeBuilder(IEnumerable<ISitemapSourceBuilder> sitemapSourceBuilders)
        {
            _sitemapSourceBuilders = sitemapSourceBuilders;
        }

        public override async Task BuildSitemapTypeAsync(Sitemap sitemap, SitemapBuilderContext context)
        {
            context.Response = new SitemapResponse
            {
                ResponseElement = new XElement(Namespace + "urlset",
                    new XAttribute(XNamespace.Xmlns + "xsi", SchemaInstance),
                    new XAttribute(SchemaInstance + "schemaLocation", SchemaLocation))
            };

            foreach (var source in sitemap.SitemapSources)
            {
                foreach (var sourceBuilder in _sitemapSourceBuilders)
                {
                    await sourceBuilder.BuildAsync(source, context);
                }
            }
        }
    }
}
