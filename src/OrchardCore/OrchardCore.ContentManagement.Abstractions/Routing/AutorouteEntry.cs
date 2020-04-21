namespace OrchardCore.ContentManagement.Routing
{
    public class AutorouteEntry
    {
        public AutorouteEntry(string contentItemId, string path, string containedContentItemId = null, string jsonPath = null)
        {
            ContentItemId = contentItemId;

            // Normalize path.
            Path = "/" + path.Trim('/');

            ContainedContentItemId = containedContentItemId;//; ?? contentItemId;
            JsonPath = jsonPath;
        }

        /// <summary>
        /// The id of the database document.
        /// </summary>
        public string ContentItemId;

        /// <summary>
        /// The path of the entry.
        /// </summary>
        public string Path;

        /// <summary>
        /// The id of an item contained within the document.
        /// May be null.
        /// </summary>
        public string ContainedContentItemId;

        /// <summary>
        /// The json path of an item contained within then document.
        /// </summary>
        public string JsonPath;
    }
}
