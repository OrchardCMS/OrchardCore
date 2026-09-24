using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.UrlRewriting;

/// <summary>
/// Declares the columns of the <see cref="UrlRewritingAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Table</c>. Each column renders one or more zones of the <c>RewriteRule_SummaryAdmin</c> shape.
/// </summary>
public sealed class UrlRewritingAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public UrlRewritingAdminListColumnProvider(IStringLocalizer<UrlRewritingAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        context.Columns.Add(new AdminListColumn
        {
            // The drag handle: rules are evaluated in order, so the order is part of the data.
            Name = "Handle",
            Position = "5",
            Zones = ["Handle"],
            Width = AdminListColumn.AutoWidth,
        });

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

        return Task.CompletedTask;
    }
}
