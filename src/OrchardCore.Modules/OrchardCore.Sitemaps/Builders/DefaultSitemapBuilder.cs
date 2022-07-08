using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public class DefaultSitemapBuilder : ISitemapBuilder
    {
        private readonly IEnumerable<ISitemapTypeBuilder> _sitemapTypeBuilders;

        public DefaultSitemapBuilder(IEnumerable<ISitemapTypeBuilder> sitemapTypeBuilders)
        {
            _sitemapTypeBuilders = sitemapTypeBuilders;
        }

        public async Task<XDocument> BuildAsync(SitemapType sitemap, SitemapBuilderContext context)
        {
            if (!sitemap.Enabled)
            {
                return null;
            }

            foreach (var sitemapTypeBuilder in _sitemapTypeBuilders)
            {
                await sitemapTypeBuilder.BuildAsync(sitemap, context);
                if (context.Response != null)
                {
                    var document = new XDocument(context.Response.ResponseElement);

                    return new XDocument(document);
                }
            }

            return null;
        }
    }
}
