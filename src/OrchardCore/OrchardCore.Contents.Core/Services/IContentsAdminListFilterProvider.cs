using OrchardCore.ContentManagement;
using YesSql.Filters.Query;

namespace OrchardCore.Contents.Services
{
    public interface IContentsAdminListFilterProvider
    {
        void Build(QueryEngineBuilder<ContentItem> builder);
    }
}
