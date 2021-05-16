using System.Threading.Tasks;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapEntries
    {
        private readonly ISitemapManager _sitemapManager;

        private SitemapRouteDocument _document;

        public SitemapEntries(ISitemapManager sitemapManager)
        {
            _sitemapManager = sitemapManager;
        }

        public async Task<(bool, string)> TryGetSitemapIdByPathAsync(string path)
        {
            var identifier = await _sitemapManager.GetIdentifierAsync();
            if (_document == null || _document.Identifier != identifier)
            {
                await BuildEntriesAsync(identifier);
            }

            if (_document.SitemapIds.TryGetValue(path, out var sitemapId))
            {
                return (true, sitemapId);
            }

            return (false, sitemapId);
        }

        public async Task<(bool, string)> TryGetPathBySitemapIdAsync(string sitemapId)
        {
            var identifier = await _sitemapManager.GetIdentifierAsync();
            if (_document == null || _document.Identifier != identifier)
            {
                await BuildEntriesAsync(identifier);
            }

            if (_document.SitemapPaths.TryGetValue(sitemapId, out var path))
            {
                return (true, path);
            }

            return (false, path);
        }

        private async Task BuildEntriesAsync(string identifier)
        {
            var document = new SitemapRouteDocument()
            {
                Identifier = identifier
            };

            var sitemaps = await _sitemapManager.GetSitemapsAsync();
            foreach (var sitemap in sitemaps)
            {
                if (!sitemap.Enabled)
                {
                    continue;
                }

                document.SitemapIds[sitemap.Path] = sitemap.SitemapId;
                document.SitemapPaths[sitemap.SitemapId] = sitemap.Path;
            }

            _document = document;
        }
    }
}
