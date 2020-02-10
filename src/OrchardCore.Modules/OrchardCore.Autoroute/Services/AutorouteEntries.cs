using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteEntries : IAutorouteEntries
    {
        private readonly Dictionary<string, string> _paths;
        private readonly Dictionary<string, string> _contentItemIds;

        public AutorouteEntries()
        {
            _paths = new Dictionary<string, string>();
            _contentItemIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetContentItemId(string path, out string contentItemId)
        {
            return _contentItemIds.TryGetValue(path.TrimEnd('/'), out contentItemId);
        }

        public bool TryGetPath(string contentItemId, out string path)
        {
            return _paths.TryGetValue(contentItemId, out path);
        }

        public void AddEntries(IEnumerable<AutorouteEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    if (_paths.TryGetValue(entry.ContentItemId, out var previousPath))
                    {
                        _contentItemIds.Remove(previousPath);
                    }

                    var requestPath = "/" + entry.Path.Trim('/');
                    _paths[entry.ContentItemId] = requestPath;
                    _contentItemIds[requestPath] = entry.ContentItemId;
                }
            }
        }

        public void RemoveEntries(IEnumerable<AutorouteEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    _paths.Remove(entry.ContentItemId);
                    _contentItemIds.Remove(entry.Path);
                }
            }
        }
    }
}
