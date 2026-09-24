using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Users;

/// <summary>
/// Declares the columns of the <see cref="UsersAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Table</c>. Each column renders one or more zones of the <c>User_SummaryAdmin</c> shape.
/// </summary>
public sealed class UsersAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public UsersAdminListColumnProvider(IStringLocalizer<UsersAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        if (context.ListName != UsersAdminList.Name)
        {
            return Task.CompletedTask;
        }

        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The user takes the space left by the other columns.
            Name = "User",
            Position = "20",
            Title = S["User"],
            Zones = ["Header", "Meta", "Content"],
        });

        context.Columns.Add(new AdminListColumn
        {
            Name = "Roles",
            Position = "30",
            Title = S["Roles"],
            Zones = ["Description"],
            Width = "30%",
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
