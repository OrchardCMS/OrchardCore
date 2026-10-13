using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Remote;

/// <summary>
/// Declares the columns of the <see cref="RemoteInstancesAdminList"/> list, used by the layouts with columns, e.g.
/// <c>Grid</c>. Each column renders one or more zones of the <c>RemoteInstance_SummaryAdmin</c> shape.
/// </summary>
public sealed class RemoteInstancesAdminListColumnDriver : AdminListColumnDriver<RemoteInstance>
{
    private readonly IStringLocalizer S;

    public RemoteInstancesAdminListColumnDriver(IStringLocalizer<RemoteInstancesAdminListColumnDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Columns(AdminListColumnsContext context)
    {
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

        return null;
    }
}
