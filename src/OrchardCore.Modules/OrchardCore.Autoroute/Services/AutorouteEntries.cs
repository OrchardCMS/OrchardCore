using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteEntries : IAutorouteEntries
    {
        private ImmutableDictionary<string, string> _paths;
        private ImmutableDictionary<string, string> _contentItemIds;

        public AutorouteEntries()
        {
            _paths = ImmutableDictionary<string, string>.Empty;
            _contentItemIds = ImmutableDictionary<string, string>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetContentItemId(string path, out string contentItemId)
        {
            return _contentItemIds.TryGetValue(path, out contentItemId);
        }

        public bool TryGetPath(string contentItemId, out string path)
        {
            return _paths.TryGetValue(contentItemId, out path);
        }

        public void AddEntries(IEnumerable<AutorouteEntry> entries)
        {
            var previousPaths = entries.Where(e => _paths.ContainsKey(e.ContentItemId)).Select(e => _paths[e.ContentItemId]);
            _contentItemIds = _contentItemIds.RemoveRange(previousPaths);

            var paths = entries.ToDictionary(e => e.ContentItemId, e => "/" + e.Path.TrimStart('/')).AsEnumerable();
            var contentItemIds = paths.ToDictionary(e => e.Value, e => e.Key).AsEnumerable();

            _paths = _paths.SetItems(paths);
            _contentItemIds = _contentItemIds.SetItems(contentItemIds);
        }

        public void RemoveEntries(IEnumerable<AutorouteEntry> entries)
        {
            _paths = _paths.RemoveRange(entries.Select(e => e.ContentItemId));
            _contentItemIds = _contentItemIds.RemoveRange(entries.Select(e => "/" + e.Path.TrimStart('/')));
        }
    }
}
