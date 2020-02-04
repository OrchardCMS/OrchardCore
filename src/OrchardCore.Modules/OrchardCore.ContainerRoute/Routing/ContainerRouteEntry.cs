namespace OrchardCore.ContainerRoute.Routing
{
    public class ContainerRouteEntry
    {
        public ContainerRouteEntry(string containerContentItemId, string path, string containedContentItemId = null, string jsonPath = null)
        {
            ContainerContentItemId = containerContentItemId;
            Path = path;
            ContainedContentItemId = containedContentItemId;
            JsonPath = jsonPath;
        }

        public string ContainerContentItemId;
        public string Path;
        public string ContainedContentItemId;
        public string JsonPath;
    }
}
