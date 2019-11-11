using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Routing;
using YesSql;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapManager : ISitemapManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;
        private readonly SitemapEntries _sitemapEntries;

        private const string SitemapsDocumentCacheKey = nameof(SitemapsDocumentCacheKey);

        public IChangeToken ChangeToken => _signal.GetToken(SitemapsDocumentCacheKey);

        public SitemapManager(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache,
            SitemapEntries sitemapEntries
            )
        {
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
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

            _session.Save(existing);

            _memoryCache.Set(SitemapsDocumentCacheKey, existing);
            _signal.SignalToken(SitemapsDocumentCacheKey);
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

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        private async Task<SitemapDocument> LoadDocumentAsync()
        {
            // TODO move to sesson helper.
            var document = await _session.Query<SitemapDocument>().FirstOrDefaultAsync();
            if (document == null)
            {
                document = new SitemapDocument();
            }
            return document;
        }


        public async Task SaveSitemapAsync(string sitemapId, SitemapType sitemap)
        {
            if (sitemap.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var existing = await LoadDocumentAsync();
            if (existing.Sitemaps.ContainsKey(sitemapId))
            {
                existing.Sitemaps.Remove(sitemapId);
            }
            existing.Sitemaps[sitemapId] = sitemap;

            _session.Save(existing);

            _memoryCache.Set(SitemapsDocumentCacheKey, existing);
            _signal.SignalToken(SitemapsDocumentCacheKey);

            BuildAllSitemapRouteEntries(existing);

            return;
        }


        private async Task<SitemapDocument> GetDocumentAsync()
        {
            SitemapDocument document;

            if (!_memoryCache.TryGetValue(SitemapsDocumentCacheKey, out document))
            {
                document = await _session.Query<SitemapDocument>().FirstOrDefaultAsync();
                if (document == null)
                {
                    document = new SitemapDocument();
                }

                foreach (var sitemap in document.Sitemaps.Values)
                {
                    sitemap.IsReadonly = true;
                }

                _memoryCache.Set(SitemapsDocumentCacheKey, document);
                _signal.SignalToken(SitemapsDocumentCacheKey);

            }

            return document;
        }

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
