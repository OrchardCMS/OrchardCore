using OrchardCore.Deployment.Remote.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Remote.Drivers;

/// <summary>
/// Builds the row of the remote instances admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="RemoteInstancesAdminList"/> renders.
/// </summary>
public sealed class RemoteInstanceDisplayDriver : DisplayDriver<RemoteInstance>
{
    public override Task<IDisplayResult> DisplayAsync(RemoteInstance instance, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RemoteInstance_Checkbox_SummaryAdmin", instance)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("RemoteInstance_Fields_SummaryAdmin", instance)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("RemoteInstance_Description_SummaryAdmin", instance)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("RemoteInstance_Buttons_SummaryAdmin", instance)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
