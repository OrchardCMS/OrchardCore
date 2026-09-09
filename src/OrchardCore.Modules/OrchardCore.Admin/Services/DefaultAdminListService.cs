using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Modules;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListService : IAdminListService
{
    private readonly IEnumerable<IAdminListColumnProvider> _columnProviders;
    private readonly IOptionsMonitor<AdminListOptions> _options;
    private readonly ILogger _logger;

    public DefaultAdminListService(
        IEnumerable<IAdminListColumnProvider> columnProviders,
        IOptionsMonitor<AdminListOptions> options,
        ILogger<DefaultAdminListService> logger)
    {
        _columnProviders = columnProviders;
        _options = options;
        _logger = logger;
    }

    // The options carry the effective defaults: the tenant configuration, overridden by the site settings.
    // They are guarded so a blank value cannot leave a list without a layout.
    private string DefaultLayout
        => string.IsNullOrWhiteSpace(_options.CurrentValue.DefaultLayout)
            ? AdminListConstants.List
            : _options.CurrentValue.DefaultLayout.Trim();

    private string DefaultActionsLayout
        => string.IsNullOrWhiteSpace(_options.CurrentValue.DefaultActionsLayout)
            ? AdminListActionsLayouts.Buttons
            : _options.CurrentValue.DefaultActionsLayout.Trim();

    public Task<string> GetLayoutAsync(string listName, string requestedLayout = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(string.IsNullOrWhiteSpace(requestedLayout)
            ? DefaultLayout
            : requestedLayout.Trim());
    }

    public Task<string> GetActionsLayoutAsync(string listName = null, string requestedLayout = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(string.IsNullOrWhiteSpace(requestedLayout)
            ? DefaultActionsLayout
            : requestedLayout.Trim());
    }

    public async Task<IList<AdminListColumn>> GetColumnsAsync(string listName, IEnumerable<AdminListColumn> defaultColumns, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        var columns = defaultColumns?.ToList() ?? [];

        // The default columns get increasing positions (10, 20, ...) when the list owner did not assign any,
        // so providers can insert a column between two of them (e.g. "15") regardless of the order they run in.
        for (var i = 0; i < columns.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(columns[i].Position))
            {
                columns[i].Position = ((i + 1) * 10).ToString(CultureInfo.InvariantCulture);
            }
        }

        var context = new AdminListColumnsContext(listName, columns);

        await _columnProviders.InvokeAsync(
            static (provider, context, cancellationToken) => provider.BuildAsync(context, cancellationToken),
            context,
            cancellationToken,
            _logger);

        // Columns added without a position go after all the positioned ones. The sort is stable, so columns
        // sharing a position keep the order they were added in.
        return context.Columns
            .OrderBy(column => string.IsNullOrWhiteSpace(column.Position) ? "after" : column.Position, FlatPositionComparer.Instance)
            .ToList();
    }
}
