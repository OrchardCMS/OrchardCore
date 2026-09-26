using OrchardCore.Deployment.Remote.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Remote.Drivers;

/// <summary>
/// Builds the row of the remote clients admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="RemoteClientsAdminList"/> renders.
/// </summary>
public sealed class RemoteClientDisplayDriver : DisplayDriver<RemoteClient>
{
    public override Task<IDisplayResult> DisplayAsync(RemoteClient client, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RemoteClient_Checkbox_SummaryAdmin", client)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("RemoteClient_Fields_SummaryAdmin", client)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("RemoteClient_Buttons_SummaryAdmin", client)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
