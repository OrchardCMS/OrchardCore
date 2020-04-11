using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Routing
{
    public static class AutorouteEntriesExtensions
    {
        public static Task AddEntryAsync(this IAutorouteEntries entries, string contentItemId, string path)
        {
            return entries.AddEntriesAsync(new[] { new AutorouteEntry { ContentItemId = contentItemId, Path = path } });
        }

        public static Task RemoveEntryAsync(this IAutorouteEntries entries, string contentItemId, string path)
        {
            return entries.RemoveEntriesAsync(new[] { new AutorouteEntry { ContentItemId = contentItemId, Path = path } });
        }
    }
}
