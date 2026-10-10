using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Lists;

/// <summary>
/// Declares the columns of the <see cref="ListPartContentsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. There is no selection column: the contained items are managed one at a time, the list has no bulk
/// actions.
/// </summary>
public sealed class ListPartContentsAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public ListPartContentsAdminListColumnProvider(IStringLocalizer<ListPartContentsAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        context.Columns.Add(new AdminListColumn
        {
            // The title takes the space left by the other columns.
            Name = "Title",
            Position = "10",
            Title = S["Title"],
            Zones = ["Title", "Header"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Type",
            Position = "20",
            Title = S["Type"],
            Zones = ["Type"],
            Width = AdminListColumn.AutoWidth,
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
            Name = "Modified",
            Position = "40",
            Title = S["Last modified"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
