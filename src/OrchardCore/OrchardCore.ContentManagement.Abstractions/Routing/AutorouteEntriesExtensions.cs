namespace OrchardCore.ContentManagement.Routing
{
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
