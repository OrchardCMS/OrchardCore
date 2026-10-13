using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Shortcodes.ViewModels;

namespace OrchardCore.Shortcodes;

/// <summary>
/// Declares the columns of the <see cref="ShortcodesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>ShortcodeTemplateEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class ShortcodesAdminListColumnDriver : AdminListColumnDriver<ShortcodeTemplateEntry>
{
    private readonly IStringLocalizer S;

    public ShortcodesAdminListColumnDriver(IStringLocalizer<ShortcodesAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name and the hint take the space left by the other columns.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Categories",
            Position = "30",
            Title = S["Categories"],
            Zones = ["Tags"],
            Width = AdminListColumn.AutoWidth,
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
