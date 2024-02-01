namespace OrchardCore.ContentManagement
{
    public interface IContentManagerSession
    {
        void Store(ContentItem item);

        bool RecallVersionId(long id, out ContentItem item);

        bool RecallPublishedItemId(string id, out ContentItem item);

        void Clear();
    }
}
