using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Admin;

/// <summary>
/// Adds the alternates that let a layout, a list or a column be overridden by a template.
/// </summary>
/// <remarks>
/// Alternates are evaluated last to first, so the most specific alternate is added last.
/// <list type="bullet">
/// <item><c>AdminList__{Layout}</c>, <c>AdminList__{Name}</c>, <c>AdminList__{Name}__{Layout}</c></item>
/// <item><c>AdminListCell__{Column}</c>, <c>AdminListCell__{Name}__{Column}</c></item>
/// <item><c>AdminListActions__{Layout}</c>, <c>AdminListActions__{Name}</c>, <c>AdminListActions__{Name}__{Layout}</c></item>
/// <item><c>AdminListToolbar__{Layout}</c>, <c>AdminListToolbar__{Name}</c>, <c>AdminListToolbar__{Name}__{Layout}</c>, and the same for <c>AdminListSearch</c></item>
/// </list>
/// Every part of a list is stamped with the name of the list it belongs to, so each of them can be overridden
/// for one list alone: the rows carry it for the actions they render, and so do the toolbar, the search bar
/// and the other regions of the layout.
/// </remarks>
public sealed class AdminListShapeTableProvider : ShapeTableProvider
{
    // The alternates of the shapes a list renders for each row, by list and layout, and by list and column. The
    // names of the lists, the layouts and the columns are declared in code, so these stay small.
    private static readonly ConcurrentDictionary<(string ListName, string Layout), string[]> _actionsAlternates = new();
    private static readonly ConcurrentDictionary<(string ListName, string Column), string[]> _cellAlternates = new();

    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe(AdminListConstants.ShapeType)
            .OnDisplaying(async context =>
            {
                var shape = context.Shape;
                var alternates = shape.Metadata.Alternates;

                shape.TryGetProperty<string>("Name", out var name);

                if (!shape.TryGetProperty<string>("Layout", out var layout) || string.IsNullOrEmpty(layout))
                {
                    layout = AdminListConstants.List;
                }

                alternates.Add($"{AdminListConstants.ShapeType}__{layout}");

                if (!string.IsNullOrEmpty(name))
                {
                    var n = name.ToSafeName();

                    alternates.Add($"{AdminListConstants.ShapeType}__{n}");
                    alternates.Add($"{AdminListConstants.ShapeType}__{n}__{layout}");

                }

                // Hand the name and the layout down to everything the layout renders, so a template can be
                // overridden for this list, or for this layout, without a page passing them to every shape.
                StampList(shape, name, layout);

                await AddCellRenderingsAsync(context, shape, name, layout);
                await AddLayoutSelectorAsync(context, shape, name, layout);
            });

        builder.Describe(AdminListActionsLayouts.ShapeType)
            .OnDisplaying(context =>
            {
                var shape = context.Shape;

                // A row template renders the actions of its row without naming the list, so the name is taken
                // from the row, which the list stamped it on.
                if (!shape.TryGetProperty<string>(AdminListConstants.ListNameProperty, out var rowListName) || string.IsNullOrEmpty(rowListName))
                {
                    if (shape.TryGetProperty<IShape>("Row", out var row) &&
                        row != null &&
                        row.TryGetProperty<string>(AdminListConstants.ListNameProperty, out rowListName) &&
                        !string.IsNullOrEmpty(rowListName))
                    {
                        shape.Properties[AdminListConstants.ListNameProperty] = rowListName;
                    }
                }

                // The layout is resolved here, before the template is selected, so the alternates can depend on it.
                if (!shape.TryGetProperty<string>("Layout", out var layout) || string.IsNullOrEmpty(layout))
                {
                    layout = context.ServiceProvider?.GetService<IOptionsMonitor<AdminListOptions>>()?.CurrentValue.DefaultActionsLayout;

                    if (string.IsNullOrEmpty(layout))
                    {
                        layout = AdminListActionsLayouts.Buttons;
                    }

                    shape.Properties["Layout"] = layout;
                }

                shape.TryGetProperty<string>(AdminListConstants.ListNameProperty, out var listName);

                // Rendered once per row, so the alternates of a list and a layout are built once.
                var alternates = _actionsAlternates.GetOrAdd((listName ?? string.Empty, layout), static key => string.IsNullOrEmpty(key.ListName)
                    ? [$"{AdminListActionsLayouts.ShapeType}__{key.Layout}"]
                    :
                    [
                        $"{AdminListActionsLayouts.ShapeType}__{key.Layout}",
                        $"{AdminListActionsLayouts.ShapeType}__{key.ListName.ToSafeName()}",
                        $"{AdminListActionsLayouts.ShapeType}__{key.ListName.ToSafeName()}__{key.Layout}",
                    ]);

                AddAlternates(shape, alternates);
            });

        // The other parts of a list, which are overridden for a layout, for a list, or for a list in a layout.
        builder.Describe(AdminListConstants.ToolbarShapeType).OnDisplaying(AddListAlternates);
        builder.Describe(AdminListConstants.SearchShapeType).OnDisplaying(AddListAlternates);

        builder.Describe(AdminListConstants.CellShapeType)
            .OnDisplaying(context =>
            {
                var shape = context.Shape;

                if (!shape.TryGetProperty<AdminListColumn>("Column", out var column) || string.IsNullOrEmpty(column.Name))
                {
                    return;
                }

                shape.TryGetProperty<string>(AdminListConstants.ListNameProperty, out var listName);

                // Rendered once per row and column, so the alternates of a list and a column are built once.
                var alternates = _cellAlternates.GetOrAdd((listName ?? string.Empty, column.Name), static key => string.IsNullOrEmpty(key.ListName)
                    ? [$"{AdminListConstants.CellShapeType}__{key.Column}"]
                    :
                    [
                        $"{AdminListConstants.CellShapeType}__{key.Column}",
                        $"{AdminListConstants.CellShapeType}__{key.ListName.ToSafeName()}__{key.Column}",
                    ]);

                AddAlternates(shape, alternates);
            });

        return ValueTask.CompletedTask;
    }

    // Tells the layouts with columns how to render the cells of each column: through an AdminListCell shape when a
    // template overrides them, otherwise directly. The List layout renders the rows whole, so it does not need it.
    private static async Task AddCellRenderingsAsync(ShapeDisplayContext context, IShape shape, string name, string layout)
    {
        var services = context.ServiceProvider;

        if (services is null ||
            string.Equals(layout, AdminListConstants.List, StringComparison.OrdinalIgnoreCase) ||
            !shape.TryGetProperty<IList<AdminListColumn>>("Columns", out var columns) ||
            columns.Count == 0)
        {
            return;
        }

        var themeManager = services.GetService<IThemeManager>();
        var shapeTableManager = services.GetService<IShapeTableManager>();

        if (themeManager is null || shapeTableManager is null)
        {
            return;
        }

        var shapeTable = await themeManager.GetShapeTableAsync(shapeTableManager);
        var cellRenderings = new AdminListCellRendering[columns.Count];

        for (var i = 0; i < columns.Count; i++)
        {
            cellRenderings[i] = AdminListCellRenderings.Get(shapeTable, name, columns[i].Name);
        }

        shape.Properties["CellRenderings"] = cellRenderings;
    }

    // Offers the user the other layouts of this list, unless the page placed a selector itself or turned it off,
    // e.g. the features, which render one list per category and offer the layout once beside their filters.
    private static async Task AddLayoutSelectorAsync(ShapeDisplayContext context, IShape shape, string name, string layout)
    {
        var services = context.ServiceProvider;

        if (services is null ||
            string.IsNullOrEmpty(name) ||
            shape.Properties.ContainsKey("LayoutSelector") ||
            (shape.TryGetProperty<bool>("ShowLayoutSelector", out var showLayoutSelector) && !showLayoutSelector))
        {
            return;
        }

        var layoutResolver = services.GetService<IAdminListLayoutResolver>();
        var shapeFactory = services.GetService<IShapeFactory>();

        if (layoutResolver is null || shapeFactory is null)
        {
            return;
        }

        var cancellationToken = services.GetService<IHttpContextAccessor>()?.HttpContext?.RequestAborted ?? CancellationToken.None;
        var layouts = await layoutResolver.GetLayoutOptionsAsync(cancellationToken);

        if (layouts.Count == 0)
        {
            return;
        }

        shape.Properties["LayoutSelector"] = await shapeFactory.CreateAsync(AdminListConstants.LayoutSelectorShapeType, Arguments.From(new
        {
            ListName = name,
            Current = layout,
            // Not "Items": a shape already exposes that name for its child shapes.
            Layouts = layouts,
        }));
    }

    // The parts of a list the layout renders beside its rows.
    private static readonly string[] _regions =
    [
        "Toolbar",
        "Header",
        "Search",
        "Actions",
        "LayoutSelector",
        "Pager",
        "PageSize",
    ];

    // Adds the alternates of a part of a list, from the least to the most specific:
    // {ShapeType}__{Layout}, {ShapeType}__{ListName}, {ShapeType}__{ListName}__{Layout},
    // e.g. AdminListSearch__Grid, AdminListSearch__Contents and AdminListSearch__Contents__Grid.
    private static void AddListAlternates(ShapeDisplayContext context)
    {
        var shape = context.Shape;
        var alternates = shape.Metadata.Alternates;
        var type = shape.Metadata.Type;

        shape.TryGetProperty<string>(AdminListConstants.ListNameProperty, out var listName);
        shape.TryGetProperty<string>(AdminListConstants.ListLayoutProperty, out var layout);

        var hasName = !string.IsNullOrEmpty(listName);
        var hasLayout = !string.IsNullOrEmpty(layout);

        if (hasLayout)
        {
            alternates.Add($"{type}__{layout.ToSafeName()}");
        }

        if (hasName)
        {
            var n = listName.ToSafeName();

            alternates.Add($"{type}__{n}");

            if (hasLayout)
            {
                alternates.Add($"{type}__{n}__{layout.ToSafeName()}");
            }
        }
    }

    // Stamps the name and the layout of the list on every part of it, unless that part carries its own.
    private static void StampList(IShape shape, string name, string layout)
    {
        foreach (var region in _regions)
        {
            if (shape.TryGetProperty<IShape>(region, out var regionShape))
            {
                SetProperty(regionShape, AdminListConstants.ListNameProperty, name);
                SetProperty(regionShape, AdminListConstants.ListLayoutProperty, layout);
            }
        }

        if (!shape.TryGetProperty<IEnumerable<IShape>>("Rows", out var rows) || rows == null)
        {
            return;
        }

        // A row carries them for the shapes it renders itself, e.g. the actions of the row. A list of rows is
        // indexed, since enumerating it through the interface boxes its enumerator.
        if (rows is IList<IShape> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                SetProperty(list[i], AdminListConstants.ListNameProperty, name);
                SetProperty(list[i], AdminListConstants.ListLayoutProperty, layout);
            }

            return;
        }

        foreach (var row in rows)
        {
            SetProperty(row, AdminListConstants.ListNameProperty, name);
            SetProperty(row, AdminListConstants.ListLayoutProperty, layout);
        }
    }

    private static void SetProperty(IShape shape, string name, string value)
    {
        if (shape != null && !string.IsNullOrEmpty(value))
        {
            shape.Properties.TryAdd(name, value);
        }
    }

    private static void AddAlternates(IShape shape, string[] alternates)
    {
        var target = shape.Metadata.Alternates;

        for (var i = 0; i < alternates.Length; i++)
        {
            target.Add(alternates[i]);
        }
    }
}
