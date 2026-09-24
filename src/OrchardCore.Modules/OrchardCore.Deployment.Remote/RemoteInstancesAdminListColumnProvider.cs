using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Deployment.Remote;

/// <summary>
/// Declares the columns of the <see cref="RemoteInstancesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Table</c>. Each column renders one or more zones of the <c>RemoteInstance_SummaryAdmin</c> shape.
/// </summary>
public sealed class RemoteInstancesAdminListColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public RemoteInstancesAdminListColumnProvider(IStringLocalizer<RemoteInstancesAdminListColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        if (context.ListName != RemoteInstancesAdminList.Name)
        {
            return Task.CompletedTask;
        }

        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The name and the url take the space left by the actions.
            Name = "Name",
            Position = "20",
            Title = S["Name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return Task.CompletedTask;
    }
}
