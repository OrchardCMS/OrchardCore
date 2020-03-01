namespace OrchardCore.ContentManagement.Routing
{
    public static class AutorouteEntriesExtensions
    {
        public static void AddEntry(this IAutorouteEntries entries, string contentItemId, string path, string containedContentItemId = null, string jsonPath = null)
        {
            entries.AddEntries(new[] { new AutorouteEntry (contentItemId, path, containedContentItemId, jsonPath) });
        }

        public static void RemoveEntry(this IAutorouteEntries entries, string contentItemId, string path, string containedContentItemId = null, string jsonPath = null)
        {
            entries.RemoveEntries(new[] { new AutorouteEntry (contentItemId, path, containedContentItemId, jsonPath) });
        }
    }
}
