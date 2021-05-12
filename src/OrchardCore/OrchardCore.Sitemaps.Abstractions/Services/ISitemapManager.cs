using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapManager
    {
        /// <summary>
        /// Gets an unique identifier that is updated when any <see cref="SitemapType"/> has changed.
        /// </summary>
        Task<string> GetIdentifierAsync();

        /// <summary>
        /// Loads all stored <see cref="SitemapType"/> for updating.
        /// </summary>
        Task<IEnumerable<SitemapType>> LoadSitemapsAsync();

        /// <summary>
        /// Gets all cached <see cref="SitemapType"/> for sharing.
        /// </summary>
        Task<IEnumerable<SitemapType>> GetSitemapsAsync();

        /// <summary>
        /// Loads the specified <see cref="SitemapType"/> for updating.
        /// </summary>
        /// <param name="sitemapId">The id of the sitemap to load.</param>
        Task<SitemapType> LoadSitemapAsync(string sitemapId);

        /// <summary>
        /// Gets the specified <see cref="SitemapType"/> for sharing.
        /// </summary>
        /// <param name="sitemapId">The id of the sitemap to get.</param>
        Task<SitemapType> GetSitemapAsync(string sitemapId);

        /// <summary>
        /// Deletes the specified <see cref="SitemapType"/>.
        /// </summary>
        /// <param name="sitemapId">The id of the sitemap to delete.</param>
        Task DeleteSitemapAsync(string sitemapId);

        /// <summary>
        /// Updates the specific <see cref="SitemapType"/>.
        /// </summary>
        Task UpdateSitemapAsync(SitemapType sitemap);

        /// <summary>
        /// Updates all <see cref="SitemapType"/>.
        /// </summary>
        Task UpdateSitemapAsync();
    }
}
