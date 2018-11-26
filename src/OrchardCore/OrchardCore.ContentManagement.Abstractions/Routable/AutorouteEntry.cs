namespace OrchardCore.ContentManagment.Routable
{
    public struct AutorouteEntry
    {
        public AutorouteEntry(string path, string contentItemId, string actualContentItemId = null, string jsonPath = null)
        {
            Path = path;
            ContentItemId = contentItemId;
            ActualContentItemId = actualContentItemId ?? contentItemId;
            JsonPath = jsonPath;
        }

        public string ContentItemId;
        public string Path;
        public string ActualContentItemId;
        public string JsonPath;
    }
}
