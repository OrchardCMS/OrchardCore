using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapManager : ISitemapManager
    {
        private readonly IDocumentManager<SitemapDocument> _documentManager;

        public SitemapManager(IDocumentManager<SitemapDocument> documentManager)
        {
            _documentManager = documentManager;
        }

        public async Task<(bool, string)> TryGetSitemapIdByPathAsync(string path)
        {
            var document = await GetDocumentAsync();

            // For backward compatibility, run only once.
            if (!document.IsBuilt)
            {
                // Load, build and update the document.
                await UpdateSitemapAsync();

                // Retrieve the updated scoped document.
                document = await LoadDocumentAsync();
            }

            if (document.SitemapIds.TryGetValue(path, out var sitemapId))
            {
                return (true, sitemapId);
            }

            return (false, sitemapId);
        }

        public async Task<(bool, string)> TryGetPathBySitemapIdAsync(string sitemapId)
        {
            var document = await GetDocumentAsync();

            // For backward compatibility, run only once.
            if (!document.IsBuilt)
            {
                // Load, build and update the document.
                await UpdateSitemapAsync();

                // Retrieve the updated scoped document.
                document = await LoadDocumentAsync();
            }

            if (document.SitemapPaths.TryGetValue(sitemapId, out var path))
            {
                return (true, path);
            }

            return (false, path);
        }

        public async Task<IEnumerable<SitemapType>> LoadSitemapsAsync()
        {
            return (await LoadDocumentAsync()).Sitemaps.Values.ToArray();
        }

        public async Task<IEnumerable<SitemapType>> GetSitemapsAsync()
        {
            return (await GetDocumentAsync()).Sitemaps.Values.ToArray();
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

        public async Task DeleteSitemapAsync(string sitemapId)
        {
            var existing = await LoadDocumentAsync();
            existing.Sitemaps.Remove(sitemapId);

            BuildEntries(existing);
            await _documentManager.UpdateAsync(existing);
        }

        public async Task UpdateSitemapAsync(SitemapType sitemap)
        {
            var existing = await LoadDocumentAsync();

            existing.Sitemaps[sitemap.SitemapId] = sitemap;
            sitemap.Identifier = IdGenerator.GenerateId();

            BuildEntries(existing);
            await _documentManager.UpdateAsync(existing);
        }

        public async Task UpdateSitemapAsync()
        {
            var existing = await LoadDocumentAsync();

            BuildEntries(existing);
            await _documentManager.UpdateAsync(existing);
        }

        private static void BuildEntries(SitemapDocument document)
        {
            document.SitemapIds.Clear();
            document.SitemapPaths.Clear();

            foreach (var sitemap in document.Sitemaps.Values)
            {
                if (!sitemap.Enabled)
                {
                    continue;
                }

                document.SitemapIds[sitemap.Path] = sitemap.SitemapId;
                document.SitemapPaths[sitemap.SitemapId] = sitemap.Path;
            }

            document.IsBuilt = true;
        }

        /// <summary>
        /// Loads the sitemap document from the store for updating and that should not be cached.
        /// </summary>
        private Task<SitemapDocument> LoadDocumentAsync() => _documentManager.GetOrCreateMutableAsync(CreateDocumentAsync);

        /// <summary>
        /// Gets the sitemap document from the cache for sharing and that should not be updated.
        /// </summary>
        private Task<SitemapDocument> GetDocumentAsync() => _documentManager.GetOrCreateImmutableAsync(CreateDocumentAsync);

        private Task<SitemapDocument> CreateDocumentAsync() => Task.FromResult(new SitemapDocument() { IsBuilt = true });
    }
}
