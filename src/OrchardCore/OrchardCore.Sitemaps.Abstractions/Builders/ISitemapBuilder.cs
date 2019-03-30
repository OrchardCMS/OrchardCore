using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public interface ISitemapBuilder
    {
        Task<XDocument> BuildAsync(SitemapNode sitemapNode, SitemapBuilderContext context);
        Task<DateTime?> ProvideLastModifiedDateAsync(SitemapNode sitemapNode, SitemapBuilderContext context);
    }
}
