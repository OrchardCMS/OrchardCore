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

        var shape = await _shapeFactory.CreateAsync(AdminListConstants.ShapeType);
        var properties = shape.Properties;

        properties["Name"] = context.Name;
        properties["Layout"] = layout.Trim();
        properties["Columns"] = columns;
        properties["Rows"] = context.Rows ?? [];

        // There is nothing to switch to when the list can only be rendered as a list.
        properties["ShowLayoutSelector"] = context.ShowLayoutSelector && hasColumns;

        // A shape reads a property it does not have as null, so the optional parts are only set when the page
        // passed them.
        SetIfNotNull(properties, "RowsAttributes", context.RowsAttributes);
        SetIfNotNull(properties, "Header", context.Header);
        SetIfNotNull(properties, "Toolbar", context.Toolbar);
        SetIfNotNull(properties, "Search", context.Search);
        SetIfNotNull(properties, "Actions", context.Actions);
        SetIfNotNull(properties, "Pager", context.Pager);
        SetIfNotNull(properties, "PageSize", context.PageSize);
        SetIfNotNull(properties, "ItemCssClass", context.ItemCssClass);
        SetIfNotNull(properties, "EmptyMessage", context.EmptyMessage);

        return shape;
    }

    private static void SetIfNotNull(IDictionary<string, object> properties, string name, object value)
    {
        if (value != null)
        {
            properties[name] = value;
        }
    }
}
