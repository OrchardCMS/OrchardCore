namespace OrchardCore.ContentManagement.Handlers
{
    public class ContentContextBase
    {
        protected ContentContextBase(ContentItem contentItem)
        {
            ContentItem = contentItem;
        }

        public ContentItem ContentItem { get; private set; }
    }
}
