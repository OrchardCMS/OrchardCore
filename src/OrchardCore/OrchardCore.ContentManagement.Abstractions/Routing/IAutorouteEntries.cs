using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Routing
{
    public interface IAutorouteEntries
    {
        bool TryGetEntryByPath(string path, out AutorouteEntry entry);
        bool TryGetEntryByContentItemId(string contentItemId, out AutorouteEntry entry);
        void AddEntries(IEnumerable<AutorouteEntry> entries);
        void RemoveEntries(IEnumerable<AutorouteEntry> entries);
    }
}
