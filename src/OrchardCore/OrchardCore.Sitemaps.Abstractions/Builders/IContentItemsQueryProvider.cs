using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public interface IContentItemsQueryProvider
    {
        /// <summary>
        /// Get content items to evaluate for inclusion in a sitemap.
        /// </summary>
        Task GetContentItemsAsync(ContentTypesSitemapSource source, ContentItemsQueryContext context);
    }
}
