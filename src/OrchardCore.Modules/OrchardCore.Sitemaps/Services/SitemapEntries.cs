using System.Collections.Generic;
using System.Collections.Immutable;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapEntries
    {
        private IImmutableDictionary<string, string> _sitemapPaths;

        public SitemapEntries()
        {
            _sitemapPaths = ImmutableDictionary<string, string>.Empty;
        }

        public bool TryGetSitemapNodeId(string path, out string sitemapNodeId)
        {
            return _sitemapPaths.TryGetValue(path, out sitemapNodeId);
        }

        public void BuildEntries(IEnumerable<SitemapEntry> entries)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            foreach (var entry in entries)
            {
                builder.Add(entry.Path, entry.SitemapId);
            }
            _sitemapPaths = builder.ToImmutable();
        }
    }
}
