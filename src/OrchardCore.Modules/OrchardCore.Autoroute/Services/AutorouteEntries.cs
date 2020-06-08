using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteEntries : IAutorouteEntries
    {
        private readonly Dictionary<string, AutorouteEntry> _paths;
        private readonly Dictionary<string, AutorouteEntry> _contentItemIds;

        public AutorouteEntries()
        {
            _paths = new Dictionary<string, AutorouteEntry>();
            _contentItemIds = new Dictionary<string, AutorouteEntry>(StringComparer.OrdinalIgnoreCase);
        }


        public bool TryGetEntryByPath(string path, out AutorouteEntry entry)
        {
            return _contentItemIds.TryGetValue(path, out entry);
        }

        public bool TryGetEntryByContentItemId(string contentItemId, out AutorouteEntry entry)
        {
            return _paths.TryGetValue(contentItemId, out entry);
        }

        public void AddEntries(IEnumerable<AutorouteEntry> entries)
        {
            lock (this)
            {
                // Evict all entries related to a container item from autoroute entries.
                // This is necessary to account for deletions, disabling of an item, or disabling routing of contained items.
                foreach (var entry in entries.Where(x => String.IsNullOrEmpty(x.ContainedContentItemId)))
                {
                    var entriesToRemove = _paths.Values.Where(x => x.ContentItemId == entry.ContentItemId && !String.IsNullOrEmpty(x.ContainedContentItemId));
                    foreach (var entryToRemove in entriesToRemove)
                    {
                        _paths.Remove(entryToRemove.ContainedContentItemId);
                        _contentItemIds.Remove(entryToRemove.Path);
                    }
                }

                foreach (var entry in entries)
                {
                    if (_paths.TryGetValue(entry.ContentItemId, out var previousContainerEntry) && String.IsNullOrEmpty(entry.ContainedContentItemId))
                    {
                        _contentItemIds.Remove(previousContainerEntry.Path);
                    }

                    if (!String.IsNullOrEmpty(entry.ContainedContentItemId) && _paths.TryGetValue(entry.ContainedContentItemId, out var previousContainedEntry))
                    {
                        _contentItemIds.Remove(previousContainedEntry.Path);
                    }

                    _contentItemIds[entry.Path] = entry;

                    if (!String.IsNullOrEmpty(entry.ContainedContentItemId))
                    {
                        _paths[entry.ContainedContentItemId] = entry;
                    }
                    else
                    {
                        _paths[entry.ContentItemId] = entry;
                    }
                }
            }
        }

        public void RemoveEntries(IEnumerable<AutorouteEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    // Evict all entries related to a container item from autoroute entries.
                    var entriesToRemove = _paths.Values.Where(x => x.ContentItemId == entry.ContentItemId && !String.IsNullOrEmpty(x.ContainedContentItemId));
                    foreach (var entryToRemove in entriesToRemove)
                    {
                        _paths.Remove(entryToRemove.ContainedContentItemId);
                        _contentItemIds.Remove(entryToRemove.Path);
                    }

                    _paths.Remove(entry.ContentItemId);
                    _contentItemIds.Remove(entry.Path);
                }
            }
        }
    }
}
