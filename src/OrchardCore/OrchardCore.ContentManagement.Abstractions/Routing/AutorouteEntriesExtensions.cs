using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Routing
{
    public static class AutorouteEntriesExtensions
    {
        public static Task AddEntryAsync(this IAutorouteEntries entries, string contentItemId, string path, string containedContentItemId = null, string jsonPath = null)
        {
            return entries.AddEntriesAsync(new[] { new AutorouteEntry(contentItemId, path, containedContentItemId, jsonPath) });
        }

        public static Task RemoveEntryAsync(this IAutorouteEntries entries, string contentItemId, string path, string containedContentItemId = null, string jsonPath = null)
        {
            return entries.RemoveEntriesAsync(new[] { new AutorouteEntry(contentItemId, path, containedContentItemId, jsonPath) });
        }
    }
}
