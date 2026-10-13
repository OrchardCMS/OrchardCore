using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundTasks;

/// <summary>
/// Declares the columns of the <see cref="BackgroundTasksAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>BackgroundTaskEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class BackgroundTasksAdminListColumnDriver : AdminListColumnDriver<BackgroundTaskEntry>
{
    private readonly IStringLocalizer S;

    public BackgroundTasksAdminListColumnDriver(IStringLocalizer<BackgroundTasksAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The title and the description take the space left by the other columns.
            Name = "Title",
            Position = "20",
            Title = S["Title"],
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

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
