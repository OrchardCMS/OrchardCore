using OrchardCore.ContentManagement;
using YesSql.Filters.Query;

namespace OrchardCore.Contents.Services
{
    /// <summary>
    /// Provides a custom parsing engine rule set for filtering content items in the contents admin list.
    /// </summary>
    public interface IContentsAdminListFilterProvider
    {
        void Build(QueryEngineBuilder<ContentItem> builder);
    }
}
