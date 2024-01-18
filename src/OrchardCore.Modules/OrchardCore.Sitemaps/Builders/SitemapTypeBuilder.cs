using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public class SitemapTypeBuilder : SitemapTypeBuilderBase<Sitemap>
    {
        private static readonly XNamespace _namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

        private readonly IEnumerable<ISitemapSourceBuilder> _sitemapSourceBuilders;

        public SitemapTypeBuilder(IEnumerable<ISitemapSourceBuilder> sitemapSourceBuilders)
        {
            _sitemapSourceBuilders = sitemapSourceBuilders;
        }

        public override async Task BuildSitemapTypeAsync(Sitemap sitemap, SitemapBuilderContext context)
        {
            context.Response = new SitemapResponse
            {
                ResponseElement = new XElement(_namespace + "urlset")
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
