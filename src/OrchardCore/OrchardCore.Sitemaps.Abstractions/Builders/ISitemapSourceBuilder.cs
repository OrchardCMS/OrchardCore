using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Builds items for a sitemap source.
    /// </summary>
    public interface ISitemapSourceBuilder
    {
        Task BuildAsync(SitemapSource source, SitemapBuilderContext context);
    }
}
