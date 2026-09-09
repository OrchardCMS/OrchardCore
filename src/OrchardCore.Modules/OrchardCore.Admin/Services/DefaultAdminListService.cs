using System.Globalization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListService : IAdminListService
{
    private readonly ISiteService _siteService;
    private readonly IEnumerable<IAdminListColumnProvider> _columnProviders;
    private readonly ILogger _logger;

    public DefaultAdminListService(
        ISiteService siteService,
        IEnumerable<IAdminListColumnProvider> columnProviders,
        ILogger<DefaultAdminListService> logger)
    {
        _siteService = siteService;
        _columnProviders = columnProviders;
        _logger = logger;
    }

    public async Task<string> GetLayoutAsync(string listName, string requestedLayout = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        if (!string.IsNullOrWhiteSpace(requestedLayout))
        {
            return requestedLayout.Trim();
        }

        var settings = await _siteService.GetSettingsAsync<AdminSettings>();

        return string.IsNullOrWhiteSpace(settings.ListLayout)
            ? AdminListLayouts.List
            : settings.ListLayout.Trim();
    }

    public async Task<string> GetActionsLayoutAsync(string listName = null, string requestedLayout = null)
    {
        if (!string.IsNullOrWhiteSpace(requestedLayout))
        {
            return requestedLayout.Trim();
        }

        var settings = await _siteService.GetSettingsAsync<AdminSettings>();

        return string.IsNullOrWhiteSpace(settings.ListActionsLayout)
            ? AdminListActionsLayouts.Buttons
            : settings.ListActionsLayout.Trim();
    }

    public async Task<IList<AdminListColumn>> GetColumnsAsync(string listName, IEnumerable<AdminListColumn> defaultColumns)
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

        await _columnProviders.InvokeAsync((provider, context) => provider.BuildAsync(context), context, _logger);

        // Columns added without a position go after all the positioned ones. The sort is stable, so columns
        // sharing a position keep the order they were added in.
        return context.Columns
            .OrderBy(column => string.IsNullOrWhiteSpace(column.Position) ? "after" : column.Position, FlatPositionComparer.Instance)
            .ToList();
    }
}
