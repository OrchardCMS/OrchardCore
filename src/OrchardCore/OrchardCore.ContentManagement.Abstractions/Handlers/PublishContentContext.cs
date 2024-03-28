namespace OrchardCore.ContentManagement.Handlers
{
    public class PublishContentContext : ContentContextBase
    {
        public PublishContentContext(ContentItem contentItem, ContentItem previousContentItem, bool isPublishing = true) : base(contentItem)
        {
            PublishingItem = isPublishing ? contentItem : null;
            PreviousItem = previousContentItem;
            IsPublishing = isPublishing;
        }

        public ContentItem PublishingItem { get; private set; }
        public ContentItem PreviousItem { get; private set; }

        public bool Cancel { get; set; }

        public bool IsPublishing { get; private set; } = true;
    }
}
