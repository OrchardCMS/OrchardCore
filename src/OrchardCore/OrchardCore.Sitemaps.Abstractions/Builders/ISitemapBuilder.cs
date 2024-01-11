using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Builds a sitemap.
    /// </summary>
    public interface ISitemapBuilder
    {
        Task<XDocument> BuildAsync(SitemapType sitemap, SitemapBuilderContext context);
    }
}
