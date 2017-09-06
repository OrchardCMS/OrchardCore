using System.Collections.Generic;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.Services
{
    public interface IAutorouteEntries
    {
        bool TryGetContentItemId(string path, out string contentItemId);
        bool TryGetPath(string contentItemId, out string path);
        void AddEntries(IEnumerable<AutorouteEntry> entries);
        void RemoveEntries(IEnumerable<AutorouteEntry> entries);
    }

    public static class AutorouteEntriesExtensions
    {
        public static void AddEntry(this IAutorouteEntries entries, string contentItemId, string path)
        {
            entries.AddEntries(new[] { new AutorouteEntry { ContentItemId = contentItemId, Path = path } }); 
        }

        public static void RemoveEntry(this IAutorouteEntries entries, string contentItemId, string path)
        {
            entries.RemoveEntries(new[] { new AutorouteEntry { ContentItemId = contentItemId, Path = path } });
        }
    }
}
