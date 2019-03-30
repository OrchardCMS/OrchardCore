using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public interface ISitemapNodeBuilder
    {
        Task BuildAsync(SitemapNode sitemapNode, SitemapBuilderContext context);
    }
}
