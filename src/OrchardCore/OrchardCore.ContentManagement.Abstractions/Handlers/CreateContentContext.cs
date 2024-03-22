namespace OrchardCore.ContentManagement.Handlers
{
    public class CreateContentContext : ContentContextBase
    {
        public CreateContentContext(ContentItem contentItem) : base(contentItem)
        {
            CreatingItem = contentItem;
        }

        public ContentItem CreatingItem { get; private set; }
    }
}
