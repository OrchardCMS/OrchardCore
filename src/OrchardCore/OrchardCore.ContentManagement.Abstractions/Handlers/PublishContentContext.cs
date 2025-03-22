namespace OrchardCore.ContentManagement.Handlers;

public class PublishContentContext : ContentContextBase
{
    public PublishContentContext(ContentItem contentItem, ContentItem previousContentItem) : base(contentItem)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        PublishingItem = contentItem;
#pragma warning restore CS0618 // Type or member is obsolete
        PreviousItem = previousContentItem;
    }

    [Obsolete("This method is obsolete and will be removed in future releases.")]
    public ContentItem PublishingItem { get; set; }

    public ContentItem PreviousItem { get; set; }

    public bool Cancel { get; set; }
}
