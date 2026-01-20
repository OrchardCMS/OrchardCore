namespace OrchardCore.ContentManagement.Handlers
{
    public class VersionContentContext : ContentContextBase
    {
        public VersionContentContext(ContentItem contentItem, ContentItem buildingContentItem) : base(contentItem)
        {
            BuildingContentItem = buildingContentItem;
        }

        public ContentItem BuildingContentItem { get; }
    }
}
