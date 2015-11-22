using Orchard.DependencyInjection;

namespace Orchard.ContentManagement
{
    public interface IContentManagerSession : IDependency
    {
        void Store(ContentItem item);
        bool RecallVersionId(int id, out ContentItem item);
        bool RecallContentItemId(int id, int version, out ContentItem item);
        bool RecallPublishedItemId(int id, out ContentItem item);

        void Clear();
    }
}