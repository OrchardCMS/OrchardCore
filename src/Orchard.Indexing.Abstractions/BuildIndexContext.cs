using Orchard.ContentManagement;

namespace Orchard.Indexing
{
    public class BuildIndexContext
    {
        public BuildIndexContext(
            DocumentIndex documentIndex, 
            ContentItem contentItem, 
            string key)
        {
            ContentItem = contentItem;
            DocumentIndex = documentIndex;
            Key = key;
        }

        public string Key { get; }
        public ContentItem ContentItem { get; }
        public DocumentIndex DocumentIndex { get; }
    }
}
