using System;
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

        public bool TryGetSitemapIdByPath(string path, out string sitemapId)
        {
            return _sitemapIds.TryGetValue(path, out sitemapId);
        }

        public bool TryGetPathBySitemapId(string sitemapId, out string path)
        {
            return _sitemapPaths.TryGetValue(sitemapId, out path);
        }

        public void BuildEntries(IEnumerable<SitemapEntry> entries)
        {
            var pathBuilder = ImmutableDictionary.CreateBuilder<string, string>();
            var idBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                pathBuilder.Add(entry.SitemapId, entry.Path);
                idBuilder.Add(entry.Path, entry.SitemapId);
            }
            _sitemapPaths = pathBuilder.ToImmutable();
            _sitemapIds = idBuilder.ToImmutable();
        }
    }
}
