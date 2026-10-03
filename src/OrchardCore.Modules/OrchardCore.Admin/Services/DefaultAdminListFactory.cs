using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListFactory : IAdminListFactory
{
    private readonly IAdminListLayoutResolver _layoutResolver;
    private readonly IAdminListColumnsBuilder _columnsBuilder;
    private readonly IShapeFactory _shapeFactory;
    private readonly ILogger _logger;

    public DefaultAdminListFactory(
        IAdminListLayoutResolver layoutResolver,
        IAdminListColumnsBuilder columnsBuilder,
        IShapeFactory shapeFactory,
        ILogger<DefaultAdminListFactory> logger)
    {
        _layoutResolver = layoutResolver;
        _columnsBuilder = columnsBuilder;
        _shapeFactory = shapeFactory;
        _logger = logger;
    }

    public async Task<IShape> CreateAsync(AdminListContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var columns = await _columnsBuilder.BuildAsync(context.Name, context.Data, cancellationToken);
        var hasColumns = columns.Count > 0;

        if (!hasColumns)
        {
            _logger.LogWarning("No provider declared the columns of the '{ListName}' admin list, so it can only be rendered with the '{Layout}' layout.", context.Name, AdminListConstants.List);
        }

        var layout = context.Layout;

        if (string.IsNullOrWhiteSpace(layout))
        {
            // Without columns, the List layout is the only one that can render the rows.
            layout = hasColumns
                ? await _layoutResolver.GetLayoutAsync(context.Name, cancellationToken)
                : AdminListConstants.List;
        }

        var rows = context.Rows ?? [];
        var toolbar = context.Toolbar;

        // A list without an options editor or a toolbar of its own gets the generic one.
        if (toolbar is null && context.Header is null && context.ShowToolbar)
        {
            // The toolbar counts the rows, which the layout then enumerates again, so rows that can only be
            // enumerated once are kept.
            if (!rows.TryGetNonEnumeratedCount(out var itemsCount))
            {
                var materialized = rows.ToList();
                rows = materialized;
                itemsCount = materialized.Count;
            }

            toolbar = await CreateToolbarAsync(context, itemsCount);
        }

        var shape = await _shapeFactory.CreateAsync(AdminListConstants.ShapeType);
        var properties = shape.Properties;

        properties["Name"] = context.Name;
        properties["Layout"] = layout.Trim();
        properties["Columns"] = columns;
        properties["Rows"] = rows;

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
