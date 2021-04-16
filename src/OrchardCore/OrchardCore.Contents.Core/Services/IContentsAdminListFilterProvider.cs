using OrchardCore.ContentManagement;
using OrchardCore.Filters.Query;

namespace OrchardCore.Contents.Services
{
    public interface IContentsAdminListFilterProvider
    {
        void Build(QueryEngineBuilder<ContentItem> builder);
    }
}
