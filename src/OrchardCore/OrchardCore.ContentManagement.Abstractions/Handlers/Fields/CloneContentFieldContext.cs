namespace OrchardCore.ContentManagement.Handlers;

public class CloneContentFieldContext : ContentFieldContextBase
{
    public ContentItem CloneContentItem { get; set; }

    public CloneContentFieldContext(ContentItem contentItem, ContentItem cloneContentItem)
        : base(contentItem)
    {
        CloneContentItem = cloneContentItem;
    }
}
