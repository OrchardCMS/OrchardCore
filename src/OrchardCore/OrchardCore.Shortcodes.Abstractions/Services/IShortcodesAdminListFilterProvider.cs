using OrchardCore.Data.Documents;
using YesSql.Filters.Query;

namespace OrchardCore.Shortcodes.Services;

/// <summary>
/// Provides a custom parsing engine rule set for filtering content items in the contents admin list.
/// </summary>
public interface IShortcodesAdminListFilterProvider
{
    void Build(QueryEngineBuilder<IDocument> builder);
}
