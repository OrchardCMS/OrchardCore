using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Orchard.Autoroute.Model;

namespace Orchard.Autoroute.Services
{
    public class AutorouteEntries : IAutorouteEntries
    {
        private readonly ConcurrentDictionary<int, string> _paths;
        private readonly ConcurrentDictionary<string, int> _contentItemIds;

        public AutorouteEntries()
        {
            _paths = new ConcurrentDictionary<int, string>();
            _contentItemIds = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetContentItemId(string path, out int contentItemId)
        {
            return _contentItemIds.TryGetValue(path, out contentItemId);
        }

        public bool TryGetPath(int contentItemId, out string path)
        {
            return _paths.TryGetValue(contentItemId, out path);
        }

        public void AddEntries(IEnumerable<AutorouteEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    _paths.TryAdd(entry.ContentItemId, entry.Path);
                    _contentItemIds.TryAdd(entry.Path, entry.ContentItemId);
                }
            }
        }

        public void RemoveEntries(IEnumerable<AutorouteEntry> entries)
        {
            lock (this)
            {
                int c;
                string p;

                foreach (var entry in entries)
                {
                    _paths.TryRemove(entry.ContentItemId, out p);
                    _contentItemIds.TryRemove(entry.Path, out c);
                }
            }
        }
    }
}
