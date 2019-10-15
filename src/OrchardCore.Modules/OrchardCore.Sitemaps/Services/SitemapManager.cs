using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using YesSql;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapManager : ISitemapManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;
        private readonly SitemapEntries _sitemapEntries;
        private readonly IEnumerable<ISitemapBuilder> _builders;

        private const string SitemapsDocumentCacheKey = nameof(SitemapsDocumentCacheKey);

        public IChangeToken ChangeToken => _signal.GetToken(SitemapsDocumentCacheKey);

        public SitemapManager(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache,
            SitemapEntries sitemapEntries,
            IEnumerable<ISitemapBuilder> builders)
        {
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
            _sitemapEntries = sitemapEntries;
            _builders = builders;
        }

        public async Task BuildAllSitemapRouteEntriesAsync()
        {
            var document = await GetDocumentAsync();
            var entries = document.Sitemaps.Values.Where(x => x.Enabled)
                .Select(sitemap => new SitemapEntry { Path = sitemap.Path, SitemapId = sitemap.Id });
            _sitemapEntries.BuildEntries(entries);
        }

        public async Task DeleteSitemapAsync(string id)
        {
            var document = await GetDocumentAsync();

            if (document.Sitemaps.ContainsKey(id))
            {
                document.Sitemaps.Remove(id);
            }

            _session.Save(document);

            _memoryCache.Set(SitemapsDocumentCacheKey, document);
            _signal.SignalToken(SitemapsDocumentCacheKey);

            BuildAllSitemapRouteEntries(document);

            return;
        }

        public async Task<Sitemap> GetSitemapAsync(string id)
        {
            var document = await GetDocumentAsync();

            if (document.Sitemaps.TryGetValue(id, out var sitemap))
            {
                return sitemap;
            }

            return null;
        }

        public async Task<IEnumerable<Sitemap>> ListSitemapsAsync()
        {
            return (await GetDocumentAsync()).Sitemaps.Values.ToList();
        }

        public async Task SaveSitemapAsync(string id, Sitemap sitemap)
        {
            var document = await GetDocumentAsync();

            document.Sitemaps[id] = sitemap;
            _session.Save(document);

            _memoryCache.Set(SitemapsDocumentCacheKey, document);
            _signal.SignalToken(SitemapsDocumentCacheKey);

            return;
        }


        public async Task<XDocument> BuildSitemapAsync(Sitemap sitemap, SitemapBuilderContext context)
        {
            foreach (var builder in _builders)
            {
                var result = await builder.BuildAsync(sitemap, context);
                if (result != null)
                    return result;
            }
            return null;
        }

        public async Task<DateTime?> GetSitemapLastModifiedDateAsync(Sitemap sitemap, SitemapBuilderContext context)
        {
            foreach (var builder in _builders)
            {
                var result = await builder.GetLastModifiedDateAsync(sitemap, context);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private async Task<SitemapDocument> GetDocumentAsync()
        {
            SitemapDocument sitemaps;

            if (!_memoryCache.TryGetValue(SitemapsDocumentCacheKey, out sitemaps))
            {
                sitemaps = await _session.Query<SitemapDocument>().FirstOrDefaultAsync();

                if (sitemaps == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(SitemapsDocumentCacheKey, out sitemaps))
                        {
                            sitemaps = new SitemapDocument();

                            _session.Save(sitemaps);
                            _memoryCache.Set(SitemapsDocumentCacheKey, sitemaps);
                            _signal.SignalToken(SitemapsDocumentCacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(SitemapsDocumentCacheKey, sitemaps);
                    _signal.SignalToken(SitemapsDocumentCacheKey);
                }
            }

            return sitemaps;
        }

        private void BuildAllSitemapRouteEntries(SitemapDocument document)
        {
            var entries = document.Sitemaps.Values.Where(x => x.Enabled)
                .Select(sitemap => new SitemapEntry { Path = sitemap.Path, SitemapId = sitemap.Id });
            _sitemapEntries.BuildEntries(entries);
        }
    }
}
