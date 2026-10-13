using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

/// <summary>
/// Declares the columns of the <see cref="IndexingAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>IndexProfile_SummaryAdmin</c> shape.
/// </summary>
public sealed class IndexingAdminListColumnDriver : AdminListColumnDriver<IndexProfile>
{
    private readonly IStringLocalizer S;

    public IndexingAdminListColumnDriver(IStringLocalizer<IndexingAdminListColumnDriver> stringLocalizer)
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

        context.Columns.Add(new AdminListColumn
        {
            Name = "Modified",
            Position = "40",
            Title = S["Last modified"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
