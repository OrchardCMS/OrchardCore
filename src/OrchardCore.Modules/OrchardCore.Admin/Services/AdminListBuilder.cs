using System.Collections;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.Services;

internal sealed class AdminListBuilder
{
    private readonly AdminListLayoutResolver _layoutResolver;
    private readonly IAdminListColumnsBuilder _columnsBuilder;
    private readonly IShapeFactory _shapeFactory;
    private readonly IOptionsMonitor<AdminListOptions> _options;
    private readonly ILogger _logger;
    private readonly AdminListRowBuilder _rowBuilder;

    public AdminListBuilder(
        AdminListLayoutResolver layoutResolver,
        IAdminListColumnsBuilder columnsBuilder,
        IShapeFactory shapeFactory,
        IOptionsMonitor<AdminListOptions> options,
        ILogger<AdminListBuilder> logger,
        AdminListRowBuilder rowBuilder)
    {
        _layoutResolver = layoutResolver;
        _columnsBuilder = columnsBuilder;
        _shapeFactory = shapeFactory;
        _options = options;
        _logger = logger;
        _rowBuilder = rowBuilder;
    }

    public async Task BuildAsync(IShape shape, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(shape);
        cancellationToken.ThrowIfCancellationRequested();

        if (shape.GetProperty<bool>("AdminListInitialized"))
        {
            return;
        }

        var name = shape.GetProperty<string>("Name");
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var requestedLayout = shape.GetProperty<AdminListLayout?>("Layout");
        if (requestedLayout.HasValue && !Enum.IsDefined(requestedLayout.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(shape), requestedLayout, "An admin list layout must be List or Grid.");
        }

        var layout = requestedLayout ?? AdminListLayout.List;
        var resolvesLayout = !requestedLayout.HasValue;
        var allowUserSelection = _options.CurrentValue.AllowUserSelection;
        var showLayoutSelector = shape.GetProperty("ShowLayoutSelector", true);
        AdminListColumnsContext columns = null;

        async Task<AdminListColumnsContext> BuildColumnsAsync()
        {
            var result = await _columnsBuilder.BuildColumnsAsync(
                name,
                shape.GetProperty<IEnumerable<AdminListColumn>>("Columns") ?? [],
                shape.GetProperty<IReadOnlyDictionary<string, object>>("Data"),
                cancellationToken);

            if (result.Columns.Count == 0)
            {
                _logger.LogWarning("No columns were declared for the '{ListName}' admin list, so it can only be rendered with the '{Layout}' layout.", name, AdminListLayout.List);
            }

            return result;
        }

        if (resolvesLayout)
        {
            if (allowUserSelection)
            {
                columns = await BuildColumnsAsync();
                layout = columns.Columns.Count > 0
                    ? _layoutResolver.GetLayout(name)
                    : AdminListLayout.List;
            }
            else
            {
                layout = _layoutResolver.GetLayout(name);
            }
        }

        var isList = layout == AdminListLayout.List;

        if (columns == null && (!isList || (allowUserSelection && showLayoutSelector)))
        {
            columns = await BuildColumnsAsync();
        }

        var hasColumns = columns?.Columns.Count > 0;

        if (!hasColumns)
        {
            layout = AdminListLayout.List;
            isList = true;
        }

        var displayType = isList
            ? OrchardCoreConstants.DisplayType.SummaryAdmin
            : OrchardCoreConstants.DisplayType.SummaryAdminGrid;
        var source = shape.GetProperty<IEnumerable>("Source");
        var rows = source != null
            ? (await _rowBuilder.BuildAsync(source, shape.GetProperty<Type>("ItemType"), displayType, cancellationToken)).ToArray()
            : [];

        var properties = shape.Properties;
        properties["Layout"] = layout;
        properties["DisplayType"] = displayType;
        properties["Rows"] = rows;
        properties["ShowLayoutSelector"] = showLayoutSelector && hasColumns;

        if (columns != null)
        {
            properties["Columns"] = columns.Columns;
        }

        if (shape.GetProperty<IShape>("Toolbar") == null &&
            shape.GetProperty<IShape>("Header") == null &&
            shape.GetProperty("ShowToolbar", true))
        {
            properties["Toolbar"] = await CreateToolbarAsync(shape, rows.Length);
        }

        properties["AdminListInitialized"] = true;
    }

    private async Task<IShape> CreateToolbarAsync(IShape list, int itemsCount)
    {
        var totalItemCount = itemsCount;
        var firstIndex = 0;

        if (list.GetProperty<IShape>("Pager") is { } pager && pager.TryGetProperty<int>("TotalItemCount", out var pagerTotal))
        {
            totalItemCount = pagerTotal;

            if (pager.TryGetProperty<int>("Page", out var page) && page > 1 && pager.TryGetProperty<int>("PageSize", out var pageSize))
            {
                firstIndex = (page - 1) * pageSize;
            }
        }

        return await _shapeFactory.CreateAsync(AdminListConstants.ToolbarShapeType, Arguments.From(new
        {
            ItemsCount = itemsCount,
            TotalItemCount = totalItemCount,
            StartIndex = itemsCount > 0 ? firstIndex + 1 : 0,
            EndIndex = firstIndex + itemsCount,
            ShowSelectAll = list.GetProperty("ShowSelectAll", true),
            BulkActions = list.GetProperty<IList<SelectListItem>>("BulkActions"),
            Actions = list.GetProperty<IShape>("ToolbarActions"),
        }));
    }
}
