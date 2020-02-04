namespace OrchardCore.ContainerRoute.Routing
{
    public static class ContainerRouteEntriesExtensions
    {
        public static void RemoveEntry(this IContainerRouteEntries entries, string containerContentItemId, string path)
        {
            entries.RemoveEntriesByContentItemId(new[] { containerContentItemId });
        }


        public static void AddEntry(this IContainerRouteEntries entries, string containerContentItemId, string path)
        {
            entries.AddEntry(containerContentItemId, path, null, null);
        }

        public static void AddEntry(this IContainerRouteEntries entries, string containerContentItemId, string path, string containedContentItemId, string jsonPath)
        {
            entries.AddEntries(new[] { new ContainerRouteEntry(containerContentItemId, path, containedContentItemId, jsonPath) });
        }

    }
}
