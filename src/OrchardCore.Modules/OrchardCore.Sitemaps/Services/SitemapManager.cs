using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Cache;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using YesSql;
using OrchardCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.Liquid;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapManager : ISitemapManager
    {
        // Path requirements for sitemaps include . as acceptable character.
        public static char[] InvalidCharactersForPath = ":?#[]@!$&'()*+,;=<>\\|%".ToCharArray();
        public const int MaxPathLength = 1024;
        public const string Prefix = "";
        public const string SitemapPathExtension = ".xml";

        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;
        private readonly SitemapEntries _sitemapEntries;
        private readonly IEnumerable<ISitemapBuilder> _builders;
        private readonly IStringLocalizer<SitemapManager> T;
        private readonly ISlugService _slugService;

        private const string SitemapsDocumentCacheKey = nameof(SitemapsDocumentCacheKey);

        public IChangeToken ChangeToken => _signal.GetToken(SitemapsDocumentCacheKey);

        public SitemapManager(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache,
            SitemapEntries sitemapEntries,
            IEnumerable<ISitemapBuilder> builders,
            IStringLocalizer<SitemapManager> localizer,
            ISlugService slugService
            )
        {
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
            _sitemapEntries = sitemapEntries;
            _builders = builders;
            _slugService = slugService;
            T = localizer;
        }

        public string GetSitemapSlug(string path)
        {
            return _slugService.Slugify(path) + SitemapPathExtension;
        }

        public async Task BuildAllSitemapRouteEntriesAsync()
        {
            var document = await GetDocumentAsync();
            BuildAllSitemapRouteEntries(document);
        }

        public async Task DeleteSitemapAsync(string id)
        {
            var existing = await GetDocumentAsync();
            existing.Sitemaps = existing.Sitemaps.Remove(id);

            _session.Save(existing);

            _memoryCache.Set(SitemapsDocumentCacheKey, existing);
            _signal.SignalToken(SitemapsDocumentCacheKey);
            //TODO we shouldn't need to build all now with Immutable
            BuildAllSitemapRouteEntries(existing);

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
            var existing = await GetDocumentAsync();
            existing.Sitemaps = existing.Sitemaps.SetItem(id, sitemap);

            _session.Save(existing);

            _memoryCache.Set(SitemapsDocumentCacheKey, existing);
            _signal.SignalToken(SitemapsDocumentCacheKey);

            BuildAllSitemapRouteEntries(existing);

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

        public async Task ValidatePathAsync(Sitemap sitemap, IUpdateModel updater)
        {
            // Keep localized text as similar to Autoroute as possible.
            if (sitemap.Path == "/")
            {
                updater.ModelState.AddModelError(Prefix, nameof(sitemap.Path), T["Your permalink can't be set to the homepage"]);
            }

            if (sitemap.Path.IndexOfAny(InvalidCharactersForPath) > -1 || sitemap.Path.IndexOf(' ') > -1)
            {
                var invalidCharactersForMessage = string.Join(", ", InvalidCharactersForPath.Select(c => $"\"{c}\""));
                updater.ModelState.AddModelError(Prefix, nameof(sitemap.Path), T["Please do not use any of the following characters in your permalink: {0}. No spaces are allowed (please use dashes or underscores instead).", invalidCharactersForMessage]);
            }

            // Precludes possibility of collision with Autoroute as Autoroute excludes . as a valid path character.
            if (!sitemap.Path.EndsWith(SitemapPathExtension))
            {
                updater.ModelState.AddModelError(Prefix, nameof(sitemap.Path), T["Your permalink must end with {0}.", SitemapPathExtension]);
            }

            if (sitemap.Path.Length > MaxPathLength)
            {
                updater.ModelState.AddModelError(Prefix, nameof(sitemap.Path), T["Your permalink is too long. The permalink can only be up to {0} characters.", MaxPathLength]);
            }

            var document = await GetDocumentAsync();
            var existingPath = document.Sitemaps.Values
                .FirstOrDefault(s => s.Path == sitemap.Path && s.Id != sitemap.Id);

            if (sitemap.Path != null && existingPath != null)
            {
                updater.ModelState.AddModelError(Prefix, nameof(sitemap.Path), T["Your permalink is already in use."]);
            }
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
            var entries = document.Sitemaps.Values
                .Where(x => x.Enabled)
                .Select(sitemap => new SitemapEntry {
                    Path = sitemap.Path,
                    SitemapId = sitemap.Id
                });

            _sitemapEntries.BuildEntries(entries);
        }
    }
}
