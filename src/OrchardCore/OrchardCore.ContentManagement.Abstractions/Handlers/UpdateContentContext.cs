namespace OrchardCore.ContentManagement.Handlers;

public class UpdateContentContext : ContentContextBase
{
    public UpdateContentContext(ContentItem contentItem) : base(contentItem)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        UpdatingItem = contentItem;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Obsolete("The property is deprecated and will be removed in a future release. Please use ContentItem instead.")]
    public ContentItem UpdatingItem { get; set; }
}
