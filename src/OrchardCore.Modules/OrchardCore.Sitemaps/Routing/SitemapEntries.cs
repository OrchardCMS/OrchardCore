using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapEntries
    {
        private readonly ISitemapManager _sitemapManager;

        private Dictionary<string, string> _sitemapIds;
        private Dictionary<string, string> _sitemapPaths;

        private string _identifier;

        public SitemapEntries(ISitemapManager sitemapManager)
        {
            _sitemapManager = sitemapManager;
        }

        public async Task<(bool, string)> TryGetSitemapIdByPathAsync(string path)
        {
            var identifier = await _sitemapManager.GetIdentifierAsync();
            if (_sitemapIds == null || _identifier != identifier)
            {
                await BuildEntriesAsync(identifier);
            }

            if (_sitemapIds.TryGetValue(path, out var sitemapId))
            {
                return (true, sitemapId);
            }

            return (false, sitemapId);
        }

        public async Task<(bool, string)> TryGetPathBySitemapIdAsync(string sitemapId)
        {
            var identifier = await _sitemapManager.GetIdentifierAsync();
            if (_sitemapPaths == null || _identifier != identifier)
            {
                await BuildEntriesAsync(identifier);
            }

            if (_sitemapPaths.TryGetValue(sitemapId, out var path))
            {
                return (true, path);
            }

            return (false, path);
        }

        private async Task BuildEntriesAsync(string identifier)
        {
            var sitemapIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sitemapPaths = new Dictionary<string, string>();

            var sitemaps = await _sitemapManager.GetSitemapsAsync();
            foreach (var sitemap in sitemaps)
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
                _identifier = identifier;
            }
        }
    }
}
