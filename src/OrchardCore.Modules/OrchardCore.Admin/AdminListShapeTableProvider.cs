using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
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
/// <item><c>AdminListToolbar__{Layout}</c>, <c>AdminListToolbar__{Name}</c>, <c>AdminListToolbar__{Name}__{Layout}</c>, and the same for <c>AdminListSearch</c></item>
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
            .OnDisplaying(async context =>
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

                }

                // Hand the name and the layout down to everything the layout renders, so a template can be
                // overridden for this list, or for this layout, without a page passing them to every shape.
                StampList(shape, name, layout);

                await AddLayoutSelectorAsync(context, shape, name, layout);
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

                var alternates = shape.Metadata.Alternates;

                alternates.Add($"{AdminListConstants.CellShapeType}__{column.Name}");

                if (shape.TryGetProperty<string>("ListName", out var listName) && !string.IsNullOrEmpty(listName))
                {
                    alternates.Add($"{AdminListConstants.CellShapeType}__{listName.ToSafeName()}__{column.Name}");
                }
            });

        return ValueTask.CompletedTask;
    }

    // Builds the selector offering the other layouts of this list, unless the site keeps the choice to itself
    // or the page built one of its own.
    private static async Task AddLayoutSelectorAsync(ShapeDisplayContext context, IShape shape, string name, string layout)
    {
        var services = context.ServiceProvider;

        if (services is null ||
            string.IsNullOrEmpty(name) ||
            shape.Properties.ContainsKey("LayoutSelector") ||
            services.GetService<IOptionsMonitor<AdminListOptions>>()?.CurrentValue.AllowUserSelection != true)
        {
            return;
        }

        var adminListService = services.GetService<IAdminListService>();
        var httpContext = services.GetService<IHttpContextAccessor>()?.HttpContext;

        if (adminListService is null || httpContext is null)
        {
            return;
        }

        var layouts = await adminListService.GetAvailableLayoutsAsync(httpContext.RequestAborted);

        // Nothing to offer when the site renders lists one way.
        if (layouts.Count < 2)
        {
            return;
        }

        var shapeFactory = services.GetService<IShapeFactory>();

        if (shapeFactory is null)
        {
            return;
        }

        shape.Properties["LayoutSelector"] = await shapeFactory.CreateAsync(AdminListConstants.LayoutSelectorShapeType, Arguments.From(new
        {
            ListName = name,
            Current = layout,
            // Not "Items": a shape already exposes that name for its child shapes.
            Layouts = layouts
                .Select(l => new AdminListLayoutOption { Name = l, Url = BuildLayoutUrl(httpContext.Request, l) })
                .ToList(),
        }));
    }

    // The same page, rendered with another layout. The rest of the query string is kept, so switching layout
    // holds on to the search, the filters and the page the user is on.
    private static string BuildLayoutUrl(HttpRequest request, string layout)
    {
        var parameters = new List<KeyValuePair<string, string>>();

        foreach (var pair in QueryHelpers.ParseQuery(request.QueryString.Value))
        {
            if (string.Equals(pair.Key, AdminListLayoutPreference.QueryKey, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var value in pair.Value)
            {
                parameters.Add(new KeyValuePair<string, string>(pair.Key, value));
            }
        }

        parameters.Add(new KeyValuePair<string, string>(AdminListLayoutPreference.QueryKey, layout));

        return QueryHelpers.AddQueryString((request.PathBase + request.Path).Value, parameters);
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

        if (shape.TryGetProperty<IEnumerable<object>>("Rows", out var rows) && rows != null)
        {
            // A row carries them for the shapes it renders itself, e.g. the actions of the row.
            foreach (var row in rows)
            {
                SetProperty(row as IShape, AdminListConstants.ListNameProperty, name);
                SetProperty(row as IShape, AdminListConstants.ListLayoutProperty, layout);
            }
        }
    }

    private static void SetProperty(IShape shape, string name, string value)
    {
        if (shape == null || string.IsNullOrEmpty(value) || shape.Properties.ContainsKey(name))
        {
            return;
        }

        shape.Properties[name] = value;
    }
}
