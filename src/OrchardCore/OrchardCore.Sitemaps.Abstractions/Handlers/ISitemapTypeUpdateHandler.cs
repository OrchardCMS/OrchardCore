using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Handlers
{
    /// <summary>
    /// Handles sitemaps updates based on their <see cref="SitemapSource"/>.
    /// </summary>
    public interface ISitemapTypeUpdateHandler
    {
        Task UpdateSitemapAsync(SitemapUpdateContext context);
    }
}
