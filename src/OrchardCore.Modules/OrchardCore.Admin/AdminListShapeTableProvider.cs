using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
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
/// <item><c>AdminListToolbar__{Name}</c> and <c>AdminListSearch__{Name}</c></item>
/// </list>
/// Every part of a list is stamped with the name of the list it belongs to, so each of them can be overridden
/// for one list alone: the rows carry it for the actions they render, and so do the toolbar, the search bar
/// and the other regions of the layout.
/// </remarks>
public sealed class AdminListShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe(AdminListConstants.ShapeType)
            .OnDisplaying(context =>
            {
                var shape = context.Shape;
                var alternates = shape.Metadata.Alternates;

                shape.TryGetProperty<string>("Name", out var name);

                if (!shape.TryGetProperty<string>("Layout", out var layout) || string.IsNullOrEmpty(layout))
                {
                    layout = AdminListConstants.DefaultLayout;
                }

                alternates.Add($"{AdminListConstants.ShapeType}__{layout}");

                if (!string.IsNullOrEmpty(name))
                {
                    var n = name.ToSafeName();

                    alternates.Add($"{AdminListConstants.ShapeType}__{n}");
                    alternates.Add($"{AdminListConstants.ShapeType}__{n}__{layout}");

                    // Hand the name down to everything the layout renders, so a template can be overridden for
                    // this list alone without every page passing the name to every shape it builds.
                    StampListName(shape, name);
                }
            });

        builder.Describe(AdminListActionsLayouts.ShapeType)
            .OnDisplaying(async context =>
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
                    shape.TryGetProperty<string>("ListName", out var name);

                    var adminListService = context.ServiceProvider?.GetService<IAdminListService>();

                    layout = adminListService != null
                        ? await adminListService.GetActionsLayoutAsync(name)
                        : AdminListActionsLayouts.Buttons;

                    shape.Properties["Layout"] = layout;
                }

                var alternates = shape.Metadata.Alternates;

                alternates.Add($"{AdminListActionsLayouts.ShapeType}__{layout}");

                if (shape.TryGetProperty<string>("ListName", out var listName) && !string.IsNullOrEmpty(listName))
                {
                    var n = listName.ToSafeName();

                    alternates.Add($"{AdminListActionsLayouts.ShapeType}__{n}");
                    alternates.Add($"{AdminListActionsLayouts.ShapeType}__{n}__{layout}");
                }
            });

        // The other parts of a list only need the name of the list they belong to.
        builder.Describe(AdminListConstants.ToolbarShapeType).OnDisplaying(AddListNameAlternate);
        builder.Describe(AdminListConstants.SearchShapeType).OnDisplaying(AddListNameAlternate);

        builder.Describe(AdminListConstants.CellShapeType)
            .OnDisplaying(context =>
            {
                var shape = context.Shape;

                if (!shape.TryGetProperty<AdminListColumn>("Column", out var column) || string.IsNullOrEmpty(column.Name))
                {
                    return;
                }

                var alternates = shape.Metadata.Alternates;

                alternates.Add($"{AdminListConstants.CellShapeType}__{column.Name}");

                if (shape.TryGetProperty<string>("ListName", out var listName) && !string.IsNullOrEmpty(listName))
                {
                    alternates.Add($"{AdminListConstants.CellShapeType}__{listName.ToSafeName()}__{column.Name}");
                }
            });

        return ValueTask.CompletedTask;
    }

    // The parts of a list the layout renders beside its rows.
    private static readonly string[] _regions =
    [
        "Toolbar",
        "Header",
        "Search",
        "Actions",
        "Pager",
        "PageSize",
    ];

    // Adds the {ShapeType}__{ListName} alternate to a part of a list, e.g. AdminListSearch__Contents.
    private static void AddListNameAlternate(ShapeDisplayContext context)
    {
        var shape = context.Shape;

        if (shape.TryGetProperty<string>(AdminListConstants.ListNameProperty, out var listName) && !string.IsNullOrEmpty(listName))
        {
            shape.Metadata.Alternates.Add($"{shape.Metadata.Type}__{listName.ToSafeName()}");
        }
    }

    // Stamps the name of the list on every part of it, unless that part already carries a name of its own.
    private static void StampListName(IShape shape, string name)
    {
        foreach (var region in _regions)
        {
            if (shape.TryGetProperty<IShape>(region, out var regionShape))
            {
                SetListName(regionShape, name);
            }
        }

        if (shape.TryGetProperty<IEnumerable<object>>("Rows", out var rows) && rows != null)
        {
            // A row carries it for the shapes it renders itself, e.g. the actions of the row.
            foreach (var row in rows)
            {
                SetListName(row as IShape, name);
            }
        }
    }

    private static void SetListName(IShape shape, string name)
    {
        if (shape == null || shape.Properties.ContainsKey(AdminListConstants.ListNameProperty))
        {
            return;
        }

        shape.Properties[AdminListConstants.ListNameProperty] = name;
    }
}
