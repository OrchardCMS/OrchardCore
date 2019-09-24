using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    /// <summary>
    /// Provides services to manage the sitemap sets.
    /// </summary>
    public interface ISitemapSetService
    {
        /// <summary>
        /// Returns all the sitemap sets
        /// </summary>
        Task<IList<Models.SitemapSet>> GetAsync();

        /// <summary>
        /// Persist a sitemap set
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        Task SaveAsync(Models.SitemapSet tree);

        /// <summary>
        /// Returns a sitemap set.
        /// </summary>
        Task<Models.SitemapSet> GetByIdAsync(string id);

        /// <summary>
        /// Get a sitemap node by id
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        Task<SitemapNode> GetSitemapNodeByIdAsync(string nodeId);

        /// <summary>
        /// Deletes a sitemap set
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>The count of deleted items</returns>
        Task<int> DeleteAsync(Models.SitemapSet tree);

        /// <summary>
        /// Gets a change token that is set when the sitemap set has changed.
        /// </summary>
        IChangeToken ChangeToken { get; }


    }
}
