using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Builds a sitemap source.
    /// </summary>
    public interface ISitemapTypeBuilder
    {
        Task BuildAsync(SitemapType sitemap, SitemapBuilderContext context);
    }
}
