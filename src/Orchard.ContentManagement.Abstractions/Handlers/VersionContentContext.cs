namespace Orchard.ContentManagement.Handlers
{
    public class VersionContentContext
    {
        public string ContentType { get; set; }
        public ContentItem ExistingContentItem { get; set; }
        public ContentItem BuildingContentItem { get; set; }
    }
}