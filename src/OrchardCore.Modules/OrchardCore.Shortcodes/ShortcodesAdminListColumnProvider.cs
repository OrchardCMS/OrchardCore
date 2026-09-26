using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Shortcodes;

/// <summary>
/// Declares the columns of the <see cref="ShortcodesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>ShortcodeTemplateEntry_SummaryAdmin</c> shape.
/// </summary>
public sealed class ShortcodesAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public ShortcodesAdminListColumnProvider(IStringLocalizer<ShortcodesAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
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

        return Task.CompletedTask;
    }
}
