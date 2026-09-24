using Microsoft.Extensions.Logging;
using OrchardCore.Admin.Models;
using OrchardCore.Modules;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListColumnsBuilder : IAdminListColumnsBuilder
{
    private readonly IEnumerable<IAdminListColumnProvider> _columnProviders;
    private readonly ILogger _logger;

    public DefaultAdminListColumnsBuilder(
        IEnumerable<IAdminListColumnProvider> columnProviders,
        ILogger<DefaultAdminListColumnsBuilder> logger)
    {
        _columnProviders = columnProviders;
        _logger = logger;
    }

    public async Task<IList<AdminListColumn>> BuildAsync(string listName, IReadOnlyDictionary<string, object> data = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        var context = new AdminListColumnsContext(listName, data);

        // The providers run in the order their features depend on each other, so the module owning the list adds
        // its columns before the modules depending on it change or remove them. A provider adding a column the list
        // already has throws, which is logged, and the columns it added before are kept.
        await _columnProviders.InvokeAsync(
            static (provider, context, cancellationToken) => provider.BuildAsync(context, cancellationToken),
            context,
            cancellationToken,
            _logger);

        // Sorted in place and returned as is: the collection of the providers is the one the list renders.
        var columns = context.Columns;
        columns.SortByPosition();

        // A column renders the zones it names, so one without any renders empty cells. Indexed rather than
        // enumerated: the enumerator of a Collection<T> is boxed.
        for (var i = 0; i < columns.Count; i++)
        {
            if (columns[i].Zones is not { Length: > 0 })
            {
                _logger.LogWarning("The '{ColumnName}' column of the '{ListName}' admin list renders no zone.", columns[i].Name, listName);
            }
        }

        return columns;
    }
}
