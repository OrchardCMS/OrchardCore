using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Routing;
using YesSql;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapManager : ISitemapManager
    {
        private const string SitemapsDocumentCacheKey = nameof(SitemapsDocumentCacheKey);

        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;
        private readonly ISessionHelper _sessionHelper;
        private readonly SitemapEntries _sitemapEntries;

        public SitemapManager(
            IMemoryCache memoryCache,
            ISession session,
            ISignal signal,
            ISessionHelper sessionHelper,
            SitemapEntries sitemapEntries
            )
        {
            _memoryCache = memoryCache;
            _session = session;
            _signal = signal;
            _sessionHelper = sessionHelper;
            _sitemapEntries = sitemapEntries;
        }

        public IChangeToken ChangeToken => _signal.GetToken(SitemapsDocumentCacheKey);

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
            _signal.DeferredSignalToken(SitemapsDocumentCacheKey);

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
            if (sitemap.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var existing = await LoadDocumentAsync();
            existing.Sitemaps.Remove(sitemapId);
            existing.Sitemaps[sitemapId] = sitemap;

            _session.Save(existing);
            _signal.DeferredSignalToken(SitemapsDocumentCacheKey);

            BuildAllSitemapRouteEntries(existing);

            return;
        }

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        private Task<SitemapDocument> LoadDocumentAsync() => _sessionHelper.LoadForUpdateAsync<SitemapDocument>();

        private async Task<SitemapDocument> GetDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<SitemapDocument>(SitemapsDocumentCacheKey, out var document))
            {
                var changeToken = ChangeToken;

                document = await _sessionHelper.GetForCachingAsync<SitemapDocument>();

                foreach (var sitemap in document.Sitemaps.Values)
                {
                    sitemap.IsReadonly = true;
                }

                _memoryCache.Set(SitemapsDocumentCacheKey, document, changeToken);

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
