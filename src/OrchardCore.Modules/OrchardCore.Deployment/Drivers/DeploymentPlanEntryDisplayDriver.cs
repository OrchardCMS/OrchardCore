using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Drivers;

/// <summary>
/// Builds the row of the deployment plans admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="DeploymentPlansAdminList"/> renders.
/// </summary>
public sealed class DeploymentPlanEntryDisplayDriver : DisplayDriver<DeploymentPlanEntry>
{
    public override Task<IDisplayResult> DisplayAsync(DeploymentPlanEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("DeploymentPlanEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("DeploymentPlanEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("DeploymentPlanEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
