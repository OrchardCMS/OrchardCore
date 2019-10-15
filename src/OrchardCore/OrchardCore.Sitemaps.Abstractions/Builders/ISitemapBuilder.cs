using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public interface ISitemapBuilder
    {
        Task<XDocument> BuildAsync(Sitemap sitemap, SitemapBuilderContext context);
        Task<DateTime?> GetLastModifiedDateAsync(Sitemap sitemap, SitemapBuilderContext context);
    }
}
