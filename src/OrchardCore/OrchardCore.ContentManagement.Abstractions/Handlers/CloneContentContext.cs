namespace OrchardCore.ContentManagement.Handlers
{
    public class CloneContentContext : ContentContextBase
    {
        public ContentItem CloneContentItem { get; set; }
        public string FieldName { get; set; } // TODO: check if this is required... Copied from O1
        public CloneContentContext(ContentItem contentItem, ContentItem cloneContentItem)
            : base(contentItem)
        {
            CloneContentItem = cloneContentItem;
        }
    }
}
