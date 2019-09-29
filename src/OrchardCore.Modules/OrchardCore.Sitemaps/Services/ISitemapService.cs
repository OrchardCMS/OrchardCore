using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    /// <summary>
    /// Provides services to manage the sitemap sets.
    /// </summary>
    public interface ISitemapService
    {
        /// <summary>
        /// Load the sitemap document.
        /// </summary>
        Task<SitemapDocument> LoadSitemapDocumentAsync();

        /// <summary>
        /// Persist the sitemap document.
        /// </summary>
        /// <param name="document"></param>
        void SaveSitemapDocument(SitemapDocument document);

        /// <summary>
        /// Build all the sitemap route entries.
        /// </summary>
        /// <param name="document"></param>
        void BuildAllSitemapRouteEntries(SitemapDocument document);

        /// <summary>
        /// Gets a change token that is set when the sitemap document has changed.
        /// </summary>
        IChangeToken ChangeToken { get; }
    }
}
