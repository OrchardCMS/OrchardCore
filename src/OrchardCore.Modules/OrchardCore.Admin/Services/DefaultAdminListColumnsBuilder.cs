using Microsoft.Extensions.Logging;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Zones;
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

        // Columns added without a position go after all the positioned ones. The sort is stable, so columns
        // sharing a position keep the order they were added in.
        var columns = context.Columns
            .OrderBy(column => string.IsNullOrWhiteSpace(column.Position) ? "after" : column.Position, FlatPositionComparer.Instance)
            .ToList();

        // A column renders the zones it names, so one without any renders empty cells.
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            foreach (var column in columns.Where(column => column.Zones is not { Length: > 0 }))
            {
                _logger.LogWarning("The '{ColumnName}' column of the '{ListName}' admin list renders no zone.", column.Name, listName);
            }
        }

        return columns;
    }
}
