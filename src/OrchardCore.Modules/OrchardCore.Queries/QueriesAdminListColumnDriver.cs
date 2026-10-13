using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Queries;

namespace OrchardCore.Queries;

/// <summary>
/// Declares the columns of the <see cref="QueriesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>Query_SummaryAdmin</c> shape.
/// </summary>
public sealed class QueriesAdminListColumnDriver : AdminListColumnDriver<Query>
{
    private readonly IStringLocalizer S;

    public QueriesAdminListColumnDriver(IStringLocalizer<QueriesAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name takes the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Source",
            Position = "30",
            Title = S["Source"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
