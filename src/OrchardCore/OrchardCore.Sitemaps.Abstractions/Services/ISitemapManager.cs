using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapManager
    {
        /// <summary>
        /// Returns a list of all store <see cref="Sitemap"/>.
        /// </summary>
        Task<IEnumerable<Sitemap>> ListSitemapsAsync();

        /// <summary>
        /// Saves the specific <see cref="Sitemap"/>.
        /// </summary>
        /// <param name="id">The id of the sitemap to update.</param>
        /// <param name="sitemap">The <see cref="Sitemap"/> instance to save.</param>
        Task SaveSitemapAsync(string id, Sitemap sitemap);

        /// <summary>
        /// Deletes the specified <see cref="Sitemap"/>.
        /// </summary>
        /// <param name="id">The id of the sitemap to delete.</param>
        Task DeleteSitemapAsync(string id);

        /// <summary>
        /// Gets the <see cref="Sitemap"/> instance with the specified id.
        /// </summary>
        /// <param name="id"></param>
        Task<Sitemap> GetSitemapAsync(string id);

        /// <summary>
        /// Build all the sitemap route entries.
        /// </summary>
        Task BuildAllSitemapRouteEntriesAsync();

        /// <summary>
        /// Gets a change token that is set when the sitemap document has changed.
        /// </summary>
        IChangeToken ChangeToken { get; }

        Task<XDocument> BuildSitemapAsync(Sitemap sitemap, SitemapBuilderContext context);
        Task<DateTime?> GetSitemapLastModifiedDateAsync(Sitemap sitemap, SitemapBuilderContext context);

        Task ValidatePathAsync(Sitemap sitemap, IUpdateModel updater);

        string GetSitemapSlug(string path);
    }
}
