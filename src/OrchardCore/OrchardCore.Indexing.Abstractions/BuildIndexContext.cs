using OrchardCore.ContentManagement;

namespace OrchardCore.Indexing
{
    public class BuildIndexContext
    {
        public BuildIndexContext(
            DocumentIndex documentIndex,
            ContentItem contentItem,
            string key)
        {
            DocumentIndex = documentIndex;
            ContentItem = contentItem;
            Key = key;
        }

        public string Key { get; }
        public ContentItem ContentItem { get; }
        public DocumentIndex DocumentIndex { get; }
    }
}
