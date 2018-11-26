using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagment.Routable
{
    public class AutorouteEntries : IAutorouteEntries
    {
        private readonly Dictionary<string, AutorouteEntry> _paths;
        private readonly Dictionary<string, AutorouteEntry> _contentItemIds;

        public AutorouteEntries()
        {
            _paths = new Dictionary<string, AutorouteEntry>(StringComparer.OrdinalIgnoreCase);
            _contentItemIds = new Dictionary<string, AutorouteEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetAutorouteEntryByPath(string path, out AutorouteEntry entry)
        {
            return _contentItemIds.TryGetValue(path, out entry);
        }

        public bool TryGetAutorouteEntryByContentItemId(string contentItemId, out AutorouteEntry entry)
        {
            return _paths.TryGetValue(contentItemId, out entry);
        }

        public void AddEntries(IEnumerable<AutorouteEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    _paths[entry.ContentItemId] = entry;
                    _contentItemIds["/" + entry.Path] = entry;
                }
            }
        }

        public void RemoveEntriesByPath(IEnumerable<string> paths)
        {
            lock (this)
            {
                foreach (var path in paths)
                {
                    if (_contentItemIds.TryGetValue(path, out var entry))
                    {
                        _paths.Remove(entry.ContentItemId);
                        _contentItemIds.Remove(entry.Path);
                    }
                }
            }
        }

        public void RemoveEntriesByContentItemId(IEnumerable<string> contentItemIds)
        {
            lock (this)
            {
                foreach (var contentItemId in contentItemIds)
                {
                    if (_paths.TryGetValue(contentItemId, out var entry))
                    {
                        _paths.Remove(entry.ContentItemId);
                        _contentItemIds.Remove(entry.Path);
                    }
                }
            }
        }
    }
}
