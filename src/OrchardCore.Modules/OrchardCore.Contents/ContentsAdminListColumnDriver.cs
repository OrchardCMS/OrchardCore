using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents;

/// <summary>
/// Declares the columns of the <see cref="ContentsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>Content_SummaryAdmin</c> shape.
/// </summary>
public sealed class ContentsAdminListColumnDriver : AdminListColumnDriver<ContentItem>
{
    private readonly IStringLocalizer S;

    public ContentsAdminListColumnDriver(IStringLocalizer<ContentsAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    // Other lists render content items too, e.g. the items of a list part, each with columns of its own.
    public override bool CanHandleList(string listName)
        => listName == ContentsAdminList.Name;

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The title takes the space left by the other columns.
            Name = "Title",
            Position = "20",
            Title = S["Title"],
            Zones = ["Title", "Header", "Content"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Type",
            Position = "30",
            Title = S["Type"],
            Zones = ["Type"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Status",
            Position = "40",
            Title = S["Status"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Modified",
            Position = "50",
            Title = S["Last modified"],
            Zones = ["Meta"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
