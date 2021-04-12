using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;
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
            var document = await _sitemapManager.GetDocumentAsync();
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
            var document = await _sitemapManager.GetDocumentAsync();
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
    }
}
