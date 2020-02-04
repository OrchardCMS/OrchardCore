using System.Collections.Generic;

namespace OrchardCore.ContainerRoute.Routing
{
    public interface IContainerRouteEntries
    {
        bool TryGetContainerRouteEntryByPath(string path, out ContainerRouteEntry entry);
        bool TryGetContainerRouteEntryByContentItemId(string contentItemId, out ContainerRouteEntry entry);
        void AddEntries(IEnumerable<ContainerRouteEntry> entries);
        void RemoveEntriesByPath(IEnumerable<string> paths);
        void RemoveEntriesByContentItemId(IEnumerable<string> contentItemIds);
    }
}
