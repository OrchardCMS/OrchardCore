using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Workflows;

/// <summary>
/// Declares the columns of the <see cref="WorkflowInstancesAdminList"/> list, used by the layouts with columns,
/// e.g. <c>Table</c>. Each column renders one or more zones of the <c>WorkflowEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class WorkflowInstancesAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public WorkflowInstancesAdminListColumnProvider(IStringLocalizer<WorkflowInstancesAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        if (context.ListName != WorkflowInstancesAdminList.Name)
        {
            return Task.CompletedTask;
        }

        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The workflow id takes the space left by the other columns.
            Name = "Workflow",
            Position = "20",
            Title = S["Workflow"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Status",
            Position = "30",
            Title = S["Status"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Created",
            Position = "40",
            Title = S["Created"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
