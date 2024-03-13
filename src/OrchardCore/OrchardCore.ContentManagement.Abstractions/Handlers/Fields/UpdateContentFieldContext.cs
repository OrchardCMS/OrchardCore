namespace OrchardCore.ContentManagement.Handlers;

public class UpdateContentFieldContext : ContentFieldContextBase
{
    public UpdateContentFieldContext(ContentItem contentItem) : base(contentItem)
    {
        UpdatingItem = contentItem;
    }

    public ContentItem UpdatingItem { get; set; }
}
