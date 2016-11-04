using Orchard.ContentManagement;

namespace Orchard.Indexing
{
    public class BuildIndexContext
    {
        public BuildIndexContext(DocumentIndex documentIndex, ContentItem contentItem)
        {
            ContentItem = contentItem;
            DocumentIndex = documentIndex;
        }

        public ContentItem ContentItem { get; }
        public DocumentIndex DocumentIndex { get; }
    }
}
