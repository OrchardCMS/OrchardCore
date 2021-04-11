using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapEntries
    {
        private readonly IVolatileDocumentManager<SitemapRouteDocument> _documentManager;
        private readonly IServiceProvider _serviceProvider;

        public SitemapEntries(IVolatileDocumentManager<SitemapRouteDocument> documentManager, IServiceProvider serviceProvider)
        {
            _documentManager = documentManager;
            _serviceProvider = serviceProvider;
        }

        public async Task<(bool, string)> TryGetSitemapIdByPathAsync(string path)
        {
            var document = await GetDocumentAsync();

            if (document.SitemapIds.TryGetValue(path, out var sitemapId))
            {
                return (true, sitemapId);
            }

            return (false, sitemapId);
        }

        public async Task<(bool, string)> TryGetPathBySitemapIdAsync(string sitemapId)
        {
            var document = await GetDocumentAsync();

            if (document.SitemapPaths.TryGetValue(sitemapId, out var path))
            {
                return (true, path);
            }

            return (false, path);
        }

        public async Task BuildEntriesAsync(IEnumerable<SitemapType> sitemaps)
        {
            var document = await LoadDocumentAsync();
            BuildEntries(document, sitemaps);
            await _documentManager.UpdateAsync(document);
        }

        private void BuildEntries(SitemapRouteDocument document, IEnumerable<SitemapType> sitemaps)
        {
            document.SitemapIds.Clear();
            document.SitemapPaths.Clear();

            foreach (var sitemap in sitemaps)
            {
                if (!sitemap.Enabled)
                {
                    continue;
                }

                document.SitemapIds[sitemap.Path] = sitemap.SitemapId;
                document.SitemapPaths[sitemap.SitemapId] = sitemap.Path;
            }
        }

        /// <summary>
        /// Loads the sitemap route document for updating and that should not be cached.
        /// </summary>
        private Task<SitemapRouteDocument> LoadDocumentAsync() => _documentManager.GetOrCreateMutableAsync(CreateDocumentAsync);

        /// <summary>
        /// Gets the sitemap route document for sharing and that should not be updated.
        /// </summary>
        private Task<SitemapRouteDocument> GetDocumentAsync() => _documentManager.GetOrCreateImmutableAsync(CreateDocumentAsync);

        private async Task<SitemapRouteDocument> CreateDocumentAsync()
        {
            // Lazily resolved (but only once) to prevent a circular dependency.
            var sitemapManager = _serviceProvider.GetRequiredService<ISitemapManager>();

            var sitemaps = await sitemapManager.GetSitemapsAsync();
            var document = new SitemapRouteDocument();
            BuildEntries(document, sitemaps);

            return document;
        }
    }
}
