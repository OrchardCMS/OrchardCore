using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Admin;

/// <summary>
/// Adds the alternates that let a layout, a list or a column be overridden by a template.
/// </summary>
/// <remarks>
/// Alternates are evaluated last to first, so the most specific alternate is added last.
/// <list type="bullet">
/// <item><c>AdminList__{Layout}</c>, <c>AdminList__{Name}</c>, <c>AdminList__{Name}__{Layout}</c></item>
/// <item><c>AdminListCell__{Column}</c>, <c>AdminListCell__{Name}__{Column}</c></item>
/// </list>
/// </remarks>
public sealed class AdminListShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe(AdminListLayouts.ShapeType)
            .OnDisplaying(context =>
            {
                var shape = context.Shape;
                var alternates = shape.Metadata.Alternates;

                shape.TryGetProperty<string>("Name", out var name);

                if (!shape.TryGetProperty<string>("Layout", out var layout) || string.IsNullOrEmpty(layout))
                {
                    layout = AdminListLayouts.List;
                }

                alternates.Add($"{AdminListLayouts.ShapeType}__{layout}");

                if (!string.IsNullOrEmpty(name))
                {
                    alternates.Add($"{AdminListLayouts.ShapeType}__{name}");
                    alternates.Add($"{AdminListLayouts.ShapeType}__{name}__{layout}");
                }
            });

        builder.Describe(AdminListActionsLayouts.ShapeType)
            .OnDisplaying(async context =>
            {
                var shape = context.Shape;

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
                    alternates.Add($"{AdminListActionsLayouts.ShapeType}__{listName}");
                    alternates.Add($"{AdminListActionsLayouts.ShapeType}__{listName}__{layout}");
                }
            });

        builder.Describe(AdminListLayouts.CellShapeType)
            .OnDisplaying(context =>
            {
                var shape = context.Shape;

                if (!shape.TryGetProperty<AdminListColumn>("Column", out var column) || string.IsNullOrEmpty(column.Name))
                {
                    return;
                }

                var alternates = shape.Metadata.Alternates;

                alternates.Add($"{AdminListLayouts.CellShapeType}__{column.Name}");

                if (shape.TryGetProperty<string>("ListName", out var listName) && !string.IsNullOrEmpty(listName))
                {
                    alternates.Add($"{AdminListLayouts.CellShapeType}__{listName}__{column.Name}");
                }
            });

        return ValueTask.CompletedTask;
    }
}
