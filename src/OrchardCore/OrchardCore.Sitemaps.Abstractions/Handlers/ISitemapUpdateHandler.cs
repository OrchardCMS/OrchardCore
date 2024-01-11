using System.Threading.Tasks;

namespace OrchardCore.Sitemaps.Handlers
{
    /// <summary>
    /// Handles sitemaps updates by using all <see cref="ISitemapTypeUpdateHandler"/>.
    /// </summary>
    public interface ISitemapUpdateHandler
    {
        Task UpdateSitemapAsync(SitemapUpdateContext context);
    }
}
