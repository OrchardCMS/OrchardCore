using System.Collections.Generic;
using System.Collections.Immutable;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapEntries
    {
        private IImmutableDictionary<string, string> _sitemapPaths;
        private IImmutableDictionary<string, string> _sitemapIds;

        public SitemapEntries()
        {
            _sitemapPaths = ImmutableDictionary<string, string>.Empty;
            _sitemapIds = ImmutableDictionary<string, string>.Empty;
        }

        public bool TryGetPath(string sitemapId, out string path)
        {
            return _sitemapIds.TryGetValue(sitemapId, out path);
        }

        public bool TryGetSitemapId(string path, out string sitemapId)
        {
            return _sitemapPaths.TryGetValue(path, out sitemapId);
        }

        public void BuildEntries(IEnumerable<SitemapEntry> entries)
        {
            var pathBuilder = ImmutableDictionary.CreateBuilder<string, string>();
            var idBuilder = ImmutableDictionary.CreateBuilder<string, string>();
            foreach (var entry in entries)
            {
                pathBuilder.Add(entry.Path, entry.SitemapId);
                idBuilder.Add(entry.SitemapId, entry.Path);
            }
            _sitemapPaths = pathBuilder.ToImmutable();
            _sitemapIds = idBuilder.ToImmutable();
        }
    }
}
