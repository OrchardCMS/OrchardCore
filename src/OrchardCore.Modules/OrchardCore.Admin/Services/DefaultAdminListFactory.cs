using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListFactory : IAdminListFactory
{
    private readonly IAdminListLayoutResolver _layoutResolver;
    private readonly IAdminListColumnsBuilder _columnsBuilder;
    private readonly IShapeFactory _shapeFactory;
    private readonly IOptionsMonitor<AdminListOptions> _options;
    private readonly ILogger _logger;

    public DefaultAdminListFactory(
        IAdminListLayoutResolver layoutResolver,
        IAdminListColumnsBuilder columnsBuilder,
        IShapeFactory shapeFactory,
        IOptionsMonitor<AdminListOptions> options,
        ILogger<DefaultAdminListFactory> logger)
    {
        _layoutResolver = layoutResolver;
        _columnsBuilder = columnsBuilder;
        _shapeFactory = shapeFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<IShape> CreateAsync<TModel>(AdminListContext<TModel> context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var allowUserSelection = _options.CurrentValue.AllowUserSelection;
        var layout = context.Layout;
        var resolvesLayout = string.IsNullOrWhiteSpace(layout);
        AdminListColumnsContext columns = null;

        if (resolvesLayout)
        {
            if (allowUserSelection)
            {
                // The resolver remembers the layout a user asks for, so it is only asked for a list that has
                // columns: without them, the List layout is the only one that can render the rows.
                columns = await BuildColumnsAsync(context, cancellationToken);
                layout = columns.Columns.Count > 0
                    ? await _layoutResolver.GetLayoutAsync(context.Name, cancellationToken)
                    : AdminListConstants.List;
            }
            else
            {
                layout = await _layoutResolver.GetLayoutAsync(context.Name, cancellationToken);
            }
        }

        layout = layout.Trim();

        // The List layout renders the rows whole, so its columns are only built when the list offers the other
        // layouts, which only a list with columns has.
        if (columns is null && !(IsList(layout) && !(allowUserSelection && context.ShowLayoutSelector)))
        {
            columns = await BuildColumnsAsync(context, cancellationToken);

            if (resolvesLayout && columns.Columns.Count == 0)
            {
                layout = AdminListConstants.List;
            }
        }

        var hasColumns = columns?.Columns.Count > 0;

        var rows = context.Rows ?? [];
        var rowShapes = new IShape[rows.Count];

        for (var i = 0; i < rows.Count; i++)
        {
            rowShapes[i] = rows[i].Shape;
        }

        var toolbar = context.Toolbar;

        // A list without an options editor or a toolbar of its own gets the generic one.
        if (toolbar is null && context.Header is null && context.ShowToolbar)
        {
            toolbar = await CreateToolbarAsync(context, rows.Count);
        }

        var shape = await _shapeFactory.CreateAsync(AdminListConstants.ShapeType);
        var properties = shape.Properties;

        properties["Name"] = context.Name;
        properties["Layout"] = layout;
        properties["Columns"] = hasColumns ? columns.Columns : Array.Empty<AdminListColumn>();
        properties["Rows"] = rowShapes;

        // The headers and the cells the column drivers build are only rendered by the layouts with columns.
        if (hasColumns && !IsList(layout))
        {
            properties["Headers"] = columns.Headers;
            properties["Cells"] = await _columnsBuilder.BuildCellsAsync(columns, rows, cancellationToken);
        }

        // There is nothing to switch to when the list can only be rendered as a list.
        properties["ShowLayoutSelector"] = context.ShowLayoutSelector && hasColumns;

        // A shape reads a property it does not have as null, so the optional parts are only set when the page
        // passed them.
        SetIfNotNull(properties, "RowsAttributes", context.RowsAttributes);
        SetIfNotNull(properties, "Header", context.Header);
        SetIfNotNull(properties, "Toolbar", toolbar);
        SetIfNotNull(properties, "Search", context.Search);
        SetIfNotNull(properties, "Actions", context.Actions);
        SetIfNotNull(properties, "Pager", context.Pager);
        SetIfNotNull(properties, "PageSize", context.PageSize);
        SetIfNotNull(properties, "ItemCssClass", context.ItemCssClass);
        SetIfNotNull(properties, "EmptyMessage", context.EmptyMessage);

        return shape;
    }

    private async Task<AdminListColumnsContext> BuildColumnsAsync<TModel>(AdminListContext<TModel> context, CancellationToken cancellationToken)
    {
        var columns = await _columnsBuilder.BuildColumnsAsync<TModel>(context.Name, context.Data, cancellationToken);

        if (columns.Columns.Count == 0)
        {
            _logger.LogWarning("No driver declared the columns of the '{ListName}' admin list, so it can only be rendered with the '{Layout}' layout.", context.Name, AdminListConstants.List);
        }

        return columns;
    }

    private static bool IsList(string layout)
        => string.Equals(layout, AdminListConstants.List, StringComparison.OrdinalIgnoreCase);

    // The item count of the page, and where it sits in the whole list: the pager knows the total and the page,
    // and a list without one shows all of its items.
    private async Task<IShape> CreateToolbarAsync(AdminListContext context, int itemsCount)
    {
        var totalItemCount = itemsCount;
        var firstIndex = 0;

        if (context.Pager is { } pager && pager.TryGetProperty<int>("TotalItemCount", out var pagerTotal))
        {
            totalItemCount = pagerTotal;

            if (pager.TryGetProperty<int>("Page", out var page) && page > 1 && pager.TryGetProperty<int>("PageSize", out var pageSize))
            {
                firstIndex = (page - 1) * pageSize;
            }
        }

        var toolbar = await _shapeFactory.CreateAsync(AdminListConstants.ToolbarShapeType);
        var properties = toolbar.Properties;

        properties["ItemsCount"] = itemsCount;
        properties["TotalItemCount"] = totalItemCount;
        properties["StartIndex"] = itemsCount > 0 ? firstIndex + 1 : 0;
        properties["EndIndex"] = firstIndex + itemsCount;
        properties["ShowSelectAll"] = context.ShowSelectAll;

        SetIfNotNull(properties, "BulkActions", context.BulkActions);
        SetIfNotNull(properties, "Actions", context.ToolbarActions);

        return toolbar;
    }

    private static void SetIfNotNull(IDictionary<string, object> properties, string name, object value)
    {
        if (value != null)
        {
            properties[name] = value;
        }
    }
}
