using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Sitemaps;

/// <summary>
/// Declares the columns of the <see cref="SitemapsAdminList"/> and <see cref="SitemapIndexesAdminList"/> lists,
/// used by the layouts with columns, e.g. <c>Table</c>. Both rows carry the same name and enabled state, so they
/// present the same way.
/// </summary>
public sealed class SitemapsAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public SitemapsAdminListColumnProvider(IStringLocalizer<SitemapsAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        if (context.ListName is not SitemapsAdminList.Name and not SitemapIndexesAdminList.Name)
        {
            return Task.CompletedTask;
        }

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

        return Task.CompletedTask;
    }
}
