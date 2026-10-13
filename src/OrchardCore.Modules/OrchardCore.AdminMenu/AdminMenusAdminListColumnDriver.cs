using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AdminMenu;

/// <summary>
/// Declares the columns of the <see cref="AdminMenusAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>AdminMenuEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class AdminMenusAdminListColumnDriver : AdminListColumnDriver<AdminMenuEntry>
{
    private readonly IStringLocalizer S;

    public AdminMenusAdminListColumnDriver(IStringLocalizer<AdminMenusAdminListColumnDriver> stringLocalizer)
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
