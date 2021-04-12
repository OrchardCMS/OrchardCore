using System;
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

        private Dictionary<string, string> _sitemapIds;
        private Dictionary<string, string> _sitemapPaths;

        private string _identifier;

        public SitemapManager(IDocumentManager<SitemapDocument> documentManager)
        {
            _documentManager = documentManager;
        }

        public async Task<(bool, string)> TryGetSitemapIdByPathAsync(string path)
        {
            var document = await GetDocumentAsync();
            if (_sitemapIds == null || _identifier != document.Identifier)
            {
                BuildEntries(document);
            }

            if (_sitemapIds.TryGetValue(path, out var sitemapId))
            {
                return (true, sitemapId);
            }

            return (false, sitemapId);
        }

        public async Task<(bool, string)> TryGetPathBySitemapIdAsync(string sitemapId)
        {
            var document = await GetDocumentAsync();
            if (_sitemapPaths == null || _identifier != document.Identifier)
            {
                BuildEntries(document);
            }

            if (_sitemapPaths.TryGetValue(sitemapId, out var path))
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
            await _documentManager.UpdateAsync(existing);
        }

        public async Task UpdateSitemapAsync(SitemapType sitemap)
        {
            var existing = await LoadDocumentAsync();
            existing.Sitemaps[sitemap.SitemapId] = sitemap;
            sitemap.Identifier = IdGenerator.GenerateId();
            await _documentManager.UpdateAsync(existing);
        }

        public async Task UpdateSitemapAsync()
        {
            var existing = await LoadDocumentAsync();
            await _documentManager.UpdateAsync(existing);
        }

        private void BuildEntries(SitemapDocument document)
        {
            var sitemapIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sitemapPaths = new Dictionary<string, string>();

            foreach (var sitemap in document.Sitemaps.Values)
            {
                if (!sitemap.Enabled)
                {
                    continue;
                }

                sitemapIds[sitemap.Path] = sitemap.SitemapId;
                sitemapPaths[sitemap.SitemapId] = sitemap.Path;
            }

            lock (this)
            {
                _sitemapIds = sitemapIds;
                _sitemapPaths = sitemapPaths;
                _identifier = document.Identifier;
            }
        }

        /// <summary>
        /// Loads the sitemap document from the store for updating and that should not be cached.
        /// </summary>
        private Task<SitemapDocument> LoadDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the sitemap document from the cache for sharing and that should not be updated.
        /// </summary>
        private Task<SitemapDocument> GetDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();
    }
}
