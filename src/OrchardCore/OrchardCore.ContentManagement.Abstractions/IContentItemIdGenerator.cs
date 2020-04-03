namespace OrchardCore.ContentManagement
{
    public interface IContentItemIdGenerator
    {
        string GenerateUniqueId(ContentItem contentItem);
    }
}
