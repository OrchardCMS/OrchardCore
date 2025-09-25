using OrchardCore.Shortcodes.ViewModels;
using YesSql.Filters.Enumerable;

namespace OrchardCore.Shortcodes.Services;

public interface IShortcodeFilterProvider
{
    int Order { get; }
    void Build(EnumerableEngineBuilder<DataSourceEntry> builder);
}
