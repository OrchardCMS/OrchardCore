using System;
using System.Collections.Generic;

namespace OrchardCore.ContainerRoute.Routing
{
    public class ContainerRouteEntries : IContainerRouteEntries
    {
        private readonly Dictionary<string, ContainerRouteEntry> _paths;
        private readonly Dictionary<string, ContainerRouteEntry> _contentItemIds;

        public ContainerRouteEntries()
        {
            _paths = new Dictionary<string, ContainerRouteEntry>(StringComparer.OrdinalIgnoreCase);
            _contentItemIds = new Dictionary<string, ContainerRouteEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetContainerRouteEntryByPath(string path, out ContainerRouteEntry entry)
        {
            return _contentItemIds.TryGetValue(path, out entry);
        }

        public bool TryGetContainerRouteEntryByContentItemId(string contentItemId, out ContainerRouteEntry entry)
        {
            return _paths.TryGetValue(contentItemId, out entry);
        }

        public void AddEntries(IEnumerable<ContainerRouteEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    if (!String.IsNullOrEmpty(entry.ContainedContentItemId))
                    {
                        _paths[entry.ContainedContentItemId] = entry;
                    } else
                    {
                        _paths[entry.ContainerContentItemId] = entry;
                    }
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
                        _paths.Remove(entry.ContainerContentItemId);
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
                        _paths.Remove(entry.ContainerContentItemId);
                        _contentItemIds.Remove(entry.Path);
                    }
                }
            }
        }
    }
}
