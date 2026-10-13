using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Remote;

/// <summary>
/// Declares the columns of the <see cref="RemoteClientsAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>RemoteClient_SummaryAdmin</c> shape.
/// </summary>
public sealed class RemoteClientsAdminListColumnDriver : AdminListColumnDriver<RemoteClient>
{
    private readonly IStringLocalizer S;

    public RemoteClientsAdminListColumnDriver(IStringLocalizer<RemoteClientsAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
        context.Columns.Add(AdminListColumns.Select());

        context.Columns.Add(new AdminListColumn
        {
            // The client name takes the space left by the actions.
            Name = "ClientName",
            Position = "20",
            Title = S["Client name"],
            Zones = ["Content", "Description"],
        });

        context.Columns.Add(AdminListColumns.Actions(S["Actions"]));

        return null;
    }
}
