namespace Orchard.ContentManagement
{
    public interface IContentItemIdGenerator
    {
        string GenerateUniqueId(ContentItem contentItem);
    }
}
