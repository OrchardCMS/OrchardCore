using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Routing;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapManager : ISitemapManager
    {
        private readonly IDocumentManager<SitemapDocument> _documentManager;
        private readonly SitemapEntries _sitemapEntries;

        public SitemapManager(IDocumentManager<SitemapDocument> documentManager, SitemapEntries sitemapEntries
            )
        {
            _documentManager = documentManager;
            _sitemapEntries = sitemapEntries;
        }

        public async Task BuildAllSitemapRouteEntriesAsync()
        {
            var document = await GetDocumentAsync();
            BuildAllSitemapRouteEntries(document);
        }

        public async Task DeleteSitemapAsync(string sitemapId)
        {
            var existing = await LoadDocumentAsync();
            existing.Sitemaps.Remove(sitemapId);

            await _documentManager.UpdateAsync(existing);
            BuildAllSitemapRouteEntries(existing);

            return;
        }

        public async Task<SitemapType> LoadSitemapAsync(string sitemapId)
        {
            var document = await LoadDocumentAsync();
            if (document.Sitemaps.TryGetValue(sitemapId, out var sitemap))
            {
                return sitemap;
            }

            return null;
        }

        public async Task<SitemapType> GetSitemapAsync(string sitemapId)
        {
            var document = await GetDocumentAsync();
            if (document.Sitemaps.TryGetValue(sitemapId, out var sitemap))
            {
                return sitemap;
            }

            return null;
        }

        public async Task<IEnumerable<SitemapType>> ListSitemapsAsync()
        {
            return (await GetDocumentAsync()).Sitemaps.Values;
        }

        public async Task SaveSitemapAsync(string sitemapId, SitemapType sitemap)
        {
            var existing = await LoadDocumentAsync();
            existing.Sitemaps.Remove(sitemapId);
            existing.Sitemaps[sitemapId] = sitemap;

            await _documentManager.UpdateAsync(existing);
            BuildAllSitemapRouteEntries(existing);

            return;
        }

        /// <summary>
        /// Loads the sitemap document from the store for updating and that should not be cached.
        /// </summary>
        public Task<SitemapDocument> LoadDocumentAsync() => _documentManager.GetMutableAsync();

        /// <summary>
        /// Gets the sitemap document from the cache for sharing and that should not be updated.
        /// </summary>
        public Task<SitemapDocument> GetDocumentAsync() => _documentManager.GetImmutableAsync();

        private void BuildAllSitemapRouteEntries(SitemapDocument document)
        {
            var entries = document.Sitemaps.Values
                .Where(x => x.Enabled)
                .Select(sitemap => new SitemapEntry
                {
                    Path = sitemap.Path,
                    SitemapId = sitemap.SitemapId
                });

            _sitemapEntries.BuildEntries(entries);
        }
    }
}
