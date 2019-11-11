using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapManager
    {
        Task<SitemapType> LoadSitemapAsync(string sitemapId);

        /// <summary>
        /// Returns a list of all store <see cref="SitemapType"/>.
        /// </summary>
        Task<IEnumerable<SitemapType>> ListSitemapsAsync();

        /// <summary>
        /// Saves the specific <see cref="SitemapType"/>.
        /// </summary>
        /// <param name="sitemap">The <see cref="SitemapType"/> instance to save.</param>
        Task SaveSitemapAsync(string sitemapId, SitemapType sitemap);

        /// <summary>
        /// Deletes the specified <see cref="SitemapType"/>.
        /// </summary>
        /// <param name="sitemapId">The id of the sitemap to delete.</param>
        Task DeleteSitemapAsync(string sitemapId);

        /// <summary>
        /// Gets the <see cref="SitemapType"/> instance with the specified id.
        /// </summary>
        /// <param name="sitemapId"></param>
        Task<SitemapType> GetSitemapAsync(string sitemapId);

        /// <summary>
        /// Build all the sitemap route entries.
        /// </summary>
        Task BuildAllSitemapRouteEntriesAsync();

        /// <summary>
        /// Gets a change token that is set when the sitemap document has changed.
        /// </summary>
        IChangeToken ChangeToken { get; }

    }
}
