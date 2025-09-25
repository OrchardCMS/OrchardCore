using OrchardCore.Shortcodes.ViewModels;
using YesSql.Filters.Enumerable.Services;

namespace OrchardCore.Shortcodes.Services;

public class ShortcodeQueryContext(
    IServiceProvider _serviceProvider,
    IEnumerable<DataSourceEntry> _enumerable
) : EnumerableExecutionContext<DataSourceEntry>(_enumerable)
{
    public IServiceProvider ServiceProvider { get; } = _serviceProvider;
}
