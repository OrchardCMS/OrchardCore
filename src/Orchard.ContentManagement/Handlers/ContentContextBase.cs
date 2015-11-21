namespace Orchard.ContentManagement.Handlers
{
    public class ContentContextBase
    {
        protected ContentContextBase(ContentItem contentItem)
        {
            ContentItem = contentItem;
            ContentItemId = contentItem.ContentItemId;
            ContentType = contentItem.ContentType;
        }

        public int ContentItemId { get; private set; }
        public string ContentType { get; private set; }
        public ContentItem ContentItem { get; private set; }
    }
}