namespace OrchardCore.ContentManagement.Handlers;

public class VersionContentFieldContext : ContentFieldContextBase
{
    public VersionContentFieldContext(ContentItem contentItem, ContentItem buildingContentItem)
        : base(contentItem)
    {
        BuildingContentItem = buildingContentItem;
    }

    public ContentItem BuildingContentItem { get; }
}
