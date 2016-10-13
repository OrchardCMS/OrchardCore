using System;
using System.Collections.Generic;
using Orchard.Autoroute.Model;

namespace Orchard.Autoroute.Services
{
    public class AutorouteEntries : IAutorouteEntries
    {
        private readonly Dictionary<int, string> _paths;
        private readonly Dictionary<string, int> _contentItemIds;

        public AutorouteEntries()
        {
            _paths = new Dictionary<int, string>();
            _contentItemIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
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
                    _paths[entry.ContentItemId] = entry.Path;
                    _contentItemIds[entry.Path] = entry.ContentItemId;
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
