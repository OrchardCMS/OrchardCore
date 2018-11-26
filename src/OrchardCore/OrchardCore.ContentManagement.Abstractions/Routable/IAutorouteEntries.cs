using System.Collections.Generic;

namespace OrchardCore.ContentManagment.Routable
{
    public interface IAutorouteEntries
    {
        bool TryGetAutorouteEntryByPath(string path, out AutorouteEntry entry);
        bool TryGetAutorouteEntryByContentItemId(string contentItemId, out AutorouteEntry entry);
        void AddEntries(IEnumerable<AutorouteEntry> entries);
        void RemoveEntriesByPath(IEnumerable<string> paths);
        void RemoveEntriesByContentItemId(IEnumerable<string> contentItemIds);
    }

    public static class AutorouteEntriesExtensions
    {
        public static void AddEntry(this IAutorouteEntries entries, string contentItemId, string path)
        {
            entries.AddEntry(contentItemId, path, null, null);
        }

        public static void AddEntry(this IAutorouteEntries entries, string contentItemId, string path, string actualContentItemId, string jsonPath)
        {
            entries.AddEntries(new[] { new AutorouteEntry(path, contentItemId, actualContentItemId, jsonPath) });
        }
    }
}
